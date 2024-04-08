#include <fstream>
#include <iostream>
#include <vector>
#include <opencv2/opencv.hpp>
#include <random>
#include "FastSAMPostProcessor.hpp"

using namespace std;


int main() 
{
    std::ifstream file1("preds.bin", std::ios::binary);
    std::ifstream file2("protos.bin", std::ios::binary);
    cv::Mat img = cv::imread("dogs.jpg", cv::IMREAD_COLOR);
    cv::resize(img, img, cv::Size(480, 640));

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

    FastSAMPostProcessor fSAM;
    
    auto results = fSAM.process(values1.data(), values2.data());
    cv::Mat mask = fSAM.point_prompt(results, {cv::Point(img.cols / 2, img.rows / 2)}, {1});
    fSAM.visualize(img, results);
    cv::imshow("point promt", mask);
    cv::waitKey(0);

    return 0;
}
