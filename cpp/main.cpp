#include <fstream>
#include <iostream>
#include <vector>
#include <opencv2/opencv.hpp>
#include <random>

using namespace std;


cv::Mat sigmoid(const cv::Mat& input)
{
    cv::Mat floatInput;
    input.convertTo(floatInput, CV_32F);

    cv::Mat output;
    cv::exp(-floatInput, output);
    output = 1.0 / (1.0 + output);

    return output;
}

cv::Mat segment_nms(cv::Mat& pred, cv::Size imgsz, float conf_thres=0.25, float iou_thres=0.7)
{
    std::vector<cv::Point> cands;

    cv::Mat scores_mat = pred(cv::Rect(0, 4, pred.size[1], 1));
    cv::findNonZero(scores_mat > conf_thres, cands);

    std::vector<cv::Rect> boxes;
    std::vector<float> scores;
    std::vector<vector<float>> masks;
    std::vector<int> indices;

    for (int i=0; i<cands.size(); i++)
    {
        auto pt = cands[i];
        float x = pred.at<float>(0, pt.x);
        float y = pred.at<float>(1, pt.x);
        float width = pred.at<float>(2, pt.x);
        float height = pred.at<float>(3, pt.x);
        float confidence = pred.at<float>(4, pt.x);

        auto box = cv::Rect(x - width / 2, y - height / 2, width, height);
        if (box.x < 0 || box.y < 0 || box.x + box.width >= imgsz.width || box.y + box.height >= imgsz.height)
            continue;

        boxes.push_back(box);
        scores.push_back(confidence);
        std::vector<float> mask;
        for (int j=5; j<pred.size[0]; j++)
            mask.push_back(pred.at<float>(j, pt.x));
        masks.push_back(mask);
    }

    cv::dnn::NMSBoxes(boxes, scores, conf_thres, iou_thres, indices);

    cv::Mat ret = cv::Mat::zeros(pred.size[0], indices.size(), CV_32F);
    for (int i=0; i<indices.size(); i++)
    {
        int index = indices[i];

        ret.at<float>(0, i) = boxes[index].x;
        ret.at<float>(1, i) = boxes[index].y;
        ret.at<float>(2, i) = boxes[index].width;
        ret.at<float>(3, i) = boxes[index].height;
        ret.at<float>(4, i) = scores[index];

        for (int j=5; j<pred.size[0]; j++)
            ret.at<float>(j, i) = masks[index][j-5];
    }

    return ret;
}

std::vector<pair<cv::Mat, cv::Rect>> crop_and_scale_masks(cv::Mat& masks, cv::Mat& boxes, cv::Size imgsz, int mask_size)
{
    std::vector<pair<cv::Mat, cv::Rect>> ret;
    int n_boxes = boxes.size[1];
    float height_ratio = (float)mask_size / imgsz.height;
    float width_ratio = (float)mask_size / imgsz.width;
    
    cout << "width ratio : " << width_ratio << endl;

    cv::Mat downscaled_boxes = boxes.clone();
    downscaled_boxes.row(0) *= width_ratio;
    downscaled_boxes.row(1) *= height_ratio;
    downscaled_boxes.row(2) *= width_ratio;
    downscaled_boxes.row(3) *= height_ratio;

    for (int i=0; i<n_boxes; i++)
    {
        cv::Mat box = downscaled_boxes.col(i);
        cv::Rect rect = cv::Rect(box.at<float>(0), box.at<float>(1), box.at<float>(2), box.at<float>(3));
        
        cv::Mat mask_2d(mask_size, mask_size, CV_32F, masks.row(i).ptr());
        cv::Mat m = cv::Mat::ones(mask_2d.size(), CV_8UC1) * 255;

        m(rect) = 0;
        mask_2d = mask_2d.setTo(0, m);
        
        cv::resize(mask_2d, mask_2d, imgsz, 0.0, 0.0, cv::INTER_LINEAR);
        
        rect.x /= width_ratio;
        rect.y /= height_ratio;
        rect.width /= width_ratio;
        rect.height /= height_ratio;

        ret.push_back({mask_2d > 0.5, rect});
    }

    return ret;
}

void visualize(cv::Mat& img, std::vector<pair<cv::Mat, cv::Rect>>& results)
{
    cv::Mat vis = cv::Mat::zeros(cv::Size(img.cols, img.rows), CV_8UC3);

    std::sort(results.begin(), results.end(), 
    [](pair<cv::Mat, cv::Rect>& a, pair<cv::Mat, cv::Rect>& b){
        return a.second.area() > b.second.area();
    });

    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<int> dis(0, 255);
    
    for (auto [mask, box] : results)
    {
        cv::Vec3b color(dis(gen), dis(gen), dis(gen));
        vis.setTo(color, mask);
        cv::rectangle(img, box, color, 2);
    }

    cv::imshow("img", img);
    cv::imshow("seg", vis);
    cv::waitKey(0);
}

int main() 
{
    std::ifstream file1("preds.bin", std::ios::binary);
    std::ifstream file2("protos.bin", std::ios::binary);
    cv::Mat img = cv::imread("dogs.jpg", cv::IMREAD_COLOR);
    cv::resize(img, img, cv::Size(640, 640));

    std::vector<float> values1, values2;

    if (file1.is_open()) {
        float value;
        while (file1.read(reinterpret_cast<char*>(&value), sizeof(float))) {
            values1.push_back(value);
        }
    }

    if (file2.is_open()) {
        float value;
        while (file2.read(reinterpret_cast<char*>(&value), sizeof(float))) {
            values2.push_back(value);
        }
    }

    int mask_size = 160;
    int preds_size[2] = {37, 8400};
    int protos_size[2] = {32, mask_size * mask_size};
    
    cv::Mat preds(2, preds_size, CV_32F, values1.data());
    cv::Mat protos(2, protos_size, CV_32F, values2.data());

    cv::Mat nms_result = segment_nms(preds, img.size());

    int n_boxes = nms_result.size[1];
    cv::Mat masks_coeff = nms_result(cv::Rect(0, 5, n_boxes, 32));
    cv::Mat boxes = nms_result(cv::Rect(0, 0, n_boxes, 4));

    cv::Mat masks = sigmoid(masks_coeff.t() * protos);

    auto results = crop_and_scale_masks(masks, boxes, img.size(), mask_size);
    
    visualize(img, results);
    

    return 0;
}