# Image Processing with Unity AR
This repo is one example of using image processing in mobile AR application with Unity AR Foundation. It contains how to build C++ library linking with image processing library like OpenCV and how to use the C++ library in Unity in iOS platform.


<div align="center">
    <img src="samples/FastSAM-example1.gif" alt="Segmentation example1">
    <img src="samples/FastSAM-example2.gif" alt="Segmentation example2">
</div>

Above video is the example of segmentation implemented in Unity AR and iPhone.

## Test Environment
- Unity Editor 2022.3.22f1
- Unity AR Mobile Template Project
- CMake 3.26.3
- OpenCV 4.9.0
- iPhone 15 Pro Max
<br/><br>

## Provided features
- Detect edge
- Segmentation with FastSAM
    - Test device : iPhone 15 Pro Max, model input resolution : 640x480
    - Model loading time : about 900ms when first inference is executed
    - Model inference time
        | | inference | copy array | postprocess(NMS, point prompts) | sum |
        |---|---|---|---|---|
        | time(ms) | 15~20 | 1~2 | 20~50 | 36~72 |

## Build C++ library
### iOS
1. Update Xcode with "xcode-select --install" command
2. Download and unzip OpenCV-4.9.0 iOS pack with below commands
    ```
    curl -OL https://github.com/opencv/opencv/releases/download/4.9.0/opencv-4.9.0-ios-framework.zip
    unzip opencv-4.9.0-ios-framework.zip -d cpp
    ```
3. Generate and build c++ library with install script in cpp directory
    ```
    cd cpp
    sh install.sh
    ```
4. Check the lib*.a static library file is in "Assets/Plugins/iOS" directory
<br/><br>

## Build Unity app
### iOS
1. Switch platform into iOS in Build Settings
2. Build and Run SampleScene in Build Settings
3. After Xcode project is generated, change Signing and Bundle identifier of tartget Unity-iPhone in Signing & Capabilities settings
<br/><br>