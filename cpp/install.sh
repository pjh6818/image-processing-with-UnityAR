mkdir build
cd build
cmake .. -GXcode -DCMAKE_SYSTEM_NAME=iOS -DCMAKE_OSX_DEPLOYMENT_TARGET=14.0 -DCMAKE_OSX_ARCHITECTURES=arm64
xcodebuild \
    -project ImageProcessing.xcodeproj \
    -scheme Improc \
    -destination generic/platform=iOS \
    -configuration MinSizeRel 
cp ./lib/MinSizeRel/*.a ../../Assets/Plugins/iOS/