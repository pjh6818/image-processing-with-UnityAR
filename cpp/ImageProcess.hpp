#include "FastSAMPostProcessor.hpp"

extern "C" unsigned char* detectEdge(unsigned char* rgb, int width, int height, int& o_width, int& o_height)
{
    cv::Mat rgb_mat(height, width, CV_8UC4, rgb);
    
    o_width = width / 3;
    o_height = height / 3;
    
    cv::resize(rgb_mat, rgb_mat, cv::Size(o_width, o_height));
    
    unsigned char* edge = new unsigned char[o_width * o_height];
    cv::Mat edge_mat;
    
    cv::Canny(rgb_mat, edge_mat, 50, 150);

    int temp = o_width;
    o_width = o_height;
    o_height = temp;

    cv::Mat final_edge_mat(o_height, o_width, CV_8UC1, edge);
    cv::flip(edge_mat.t(), final_edge_mat, 1);

    return edge;
}

extern "C" unsigned char* FastSAMWithPoints(int img_width, int img_height, 
                        float* preds, float* protos, cv::Point* points, int* points_label, int point_size)
{
    if (preds == nullptr || protos == nullptr)
        return nullptr;

    clock_t start = clock();
    FastSAMPostProcessor fSAM(img_width, img_height);
    auto results = fSAM.process(preds, protos);
    unsigned char* mask = new unsigned char[img_width * img_height];
    cv::Mat mask_mat(img_height, img_width, CV_8UC1, mask);

    std::vector<cv::Point> pts(points, points + point_size);
    std::vector<int> pts_label(points_label, points_label + point_size);

    cv::Mat point_mask = fSAM.point_prompt(results, pts, pts_label);
    cv::copyTo(point_mask, mask_mat, cv::noArray());
    clock_t end = clock();
    printf("post processing time %7f\n", (float)(end - start)/CLOCKS_PER_SEC);
    return mask;
}