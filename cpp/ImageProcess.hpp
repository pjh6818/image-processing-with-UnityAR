#include <opencv2/opencv.hpp>

extern "C" unsigned char* processImage(unsigned char* rgb, int width, int height, int& o_width, int& o_height)
{
    cv::Mat rgb_mat(height, width, CV_8UC4, rgb);
    
    o_width = width / 3;
    o_height = height / 3;
    
    cv::resize(rgb_mat, rgb_mat, cv::Size(o_width, o_height));
               
    unsigned char* edge = new unsigned char[o_width * o_height];
    cv::Mat edge_mat(o_height, o_width, CV_8UC1, edge);
    
    cv::Canny(rgb_mat, edge_mat, 50, 150);
    
    return edge;
}
