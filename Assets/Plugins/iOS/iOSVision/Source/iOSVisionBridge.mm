//
//  iOSVisionBridge.m
//  iOSVision
//
//  Created by 박종현 on 4/8/24.
//

#import <Foundation/Foundation.h>
#import <CoreML/CoreML.h>
#include "UnityFramework/UnityFramework-Swift.h"

CVPixelBufferRef pixelBufferFromRGBBuffer(intptr_t ptr, int width, int height, bool with_alpha) {
    
    uint8_t* rgbBuffer = (uint8_t*) ptr;
    
    NSDictionary *options = @{(NSString*)kCVPixelBufferCGImageCompatibilityKey: @(YES),
                                  (NSString*)kCVPixelBufferCGBitmapContextCompatibilityKey: @(YES)};
        
    CVPixelBufferRef pixelBuffer;
    CVReturn status = CVPixelBufferCreate(kCFAllocatorDefault, width, height, kCVPixelFormatType_32BGRA, (__bridge CFDictionaryRef) options, &pixelBuffer);
    if (status != kCVReturnSuccess) {
        NSLog(@"Failed to create CVPixelBuffer");
        return nil;
    }
    
    CVPixelBufferLockBaseAddress(pixelBuffer, 0);
    
    size_t bytesPerRow = CVPixelBufferGetBytesPerRow(pixelBuffer);
    uint8_t *baseAddress = (uint8_t*)CVPixelBufferGetBaseAddress(pixelBuffer);
    
    if (!with_alpha)
    {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                uint8_t *pixel = baseAddress + y * bytesPerRow + x * 4;
                pixel[0] = rgbBuffer[(y * width + x) * 3 + 2];
                pixel[1] = rgbBuffer[(y * width + x) * 3 + 1];
                pixel[2] = rgbBuffer[(y * width + x) * 3 + 0];
                pixel[3] = 255; // Alpha channel
            }
        }
    }
    else
    {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                uint8_t *pixel = baseAddress + y * bytesPerRow + x * 4;
                pixel[0] = rgbBuffer[(y * width + x) * 4 + 2];
                pixel[1] = rgbBuffer[(y * width + x) * 4 + 1];
                pixel[2] = rgbBuffer[(y * width + x) * 4 + 0];
                pixel[3] = rgbBuffer[(y * width + x) * 4 + 3];; // Alpha channel
            }
        }
    }
    
    CVPixelBufferUnlockBaseAddress(pixelBuffer, 0);
    
    return pixelBuffer;
}

extern "C" {
    
#pragma mark - Functions
    bool requestFastSAM(intptr_t ptr, int width, int height, bool with_alpha) {
        if (!ptr)
            return false;
        
        if (@available(iOS 16.0, *)) {
            CVPixelBufferRef pixelFrameBufferCopy = pixelBufferFromRGBBuffer(ptr, width, height, with_alpha);
            
            bool success = [[FastSAM shared] startDetectionWithBuffer:pixelFrameBufferCopy orientation:(CGImagePropertyOrientation)1];
            
            if (pixelFrameBufferCopy != nil)
                CVPixelBufferRelease(pixelFrameBufferCopy);
            return success;
        }
        return false;
    }

    bool isFastSAMDone(){
        if (@available(iOS 16.0, *)) {
            return [[FastSAM shared] Detected];
        }
        
        return false;
    }

    bool getFastSAMResult(float* preds, float* protos){
        if (@available(iOS 16.0, *)){
            if (![[FastSAM shared] Success])
                return false;
            
            clock_t start = clock();
            
            MLMultiArray *predArray = [[FastSAM shared] preds];
            MLMultiArray *protoArray = [[FastSAM shared] protos];
            
            [predArray getBytesWithHandler:^(const void *bytes, NSInteger size) {
                const float *buffer = (const float *)bytes;

                const int diff = predArray.strides[1].intValue - predArray.shape[2].intValue;
                const int dim1 = predArray.shape[1].intValue;
                const int dim2 = predArray.shape[2].intValue;
                
                for (int i=0; i<dim1; i++) {
                    for (int j=0; j<dim2; j++) {
                        int idx = i * dim2 + j;
                        preds[idx] = buffer[idx + i * diff];
                    }
                }
             }];
            
            [protoArray getBytesWithHandler:^(const void *bytes, NSInteger size) {
                const float *buffer = (const float *)bytes;

                const int diff = protoArray.strides[2].intValue - protoArray.shape[3].intValue;
                const int dim1 = protoArray.shape[1].intValue;
                const int dim2 = protoArray.shape[2].intValue;
                const int dim3 = protoArray.shape[3].intValue;
                
                for (int i=0; i<dim1; i++) {
                    for (int j=0; j<dim2; j++) {
                        for (int k=0; k<dim3; k++) {
                            int idx = i * dim2 * dim3 + j * dim3 + k;
                            protos[idx] = buffer[idx + (i * dim2 + j) * diff];
                        }
                    }
                }
             }];

            clock_t end = clock();
            printf("result copy time %7f\n", (float)(end-start) / CLOCKS_PER_SEC);
            
            return true;
        }
    
        return false;
    }
}
