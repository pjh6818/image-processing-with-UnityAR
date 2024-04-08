//
//  iOSVisionBridge.m
//  iOSVision
//
//  Created by 박종현 on 4/8/24.
//

#import <Foundation/Foundation.h>
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
        
        if (@available(iOS 15.0, *)) {
            [[FastSAM shared] reset];
            
            CVPixelBufferRef pixelFrameBufferCopy = pixelBufferFromRGBBuffer(ptr, width, height, with_alpha);
            
            bool success = [[FastSAM shared] startDetectionWithBuffer:pixelFrameBufferCopy orientation:(CGImagePropertyOrientation)6];
            
            if (pixelFrameBufferCopy != nil)
                CVPixelBufferRelease(pixelFrameBufferCopy);
            return success;
        }
        return false;
    }

    bool isFastSAMDone(){
        if (@available(iOS 15.0, *)) {
            return [[FastSAM shared] Detected];
        }
        
        return false;
    }

    bool getFastSAMResult(float* preds, float* protos){
        if (@available(iOS 15.0, *)){
            if (![[FastSAM shared] Success])
                return false;
            
            NSArray* array_pred = [[FastSAM shared] predBuffer];
            NSArray* array_proto = [[FastSAM shared] protoBuffer];
            
    
            for (int i = 0; i < 37*6300; i++){
                float f = [[array_pred objectAtIndex:i] floatValue];
                preds[i] = f;
            }
            
            for (int i=0; i<32*120*160; i++){
                float f = [[array_proto objectAtIndex:i] floatValue];
                protos[i] = f;
            }
            
            return true;
        }
    
        return false;
    }
}
