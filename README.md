# Image Processing with Unity AR
This repo is one example of using image processing in mobile AR application with Unity AR Foundation. It contains how to build C++ library linking with image processing library like OpenCV and how to use the C++ library in Unity in iOS and Android platform.
<br/><br>

## Test Environment
- Unity Editor 2022.3.22f1
- Unity AR Mobile Template Project
- CMake 3.26.3
- OpenCV 4.9.0
- iPhone 15 Pro Max
<br/><br>

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

