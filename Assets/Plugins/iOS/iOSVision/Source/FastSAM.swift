//
//  FastSAM.swift
//  Unity-iPhone
//
//  Created by 박종현 on 4/8/24.
//

import Foundation
import Vision

extension NSLock {
    func sync<T>(block: () throws -> T) rethrows -> T {
        lock()
        defer { unlock() }
        return try block()
    }
}

@available(iOS 16.0, *)
@objc public class FastSAM : NSObject {
    
    @objc public static let shared = FastSAM()
    
    private let visionQueue = DispatchQueue(label: "com.pjh.FastSAM")
    
    private var retainedBuffer: CVPixelBuffer?
    
    private var start: CFAbsoluteTime = 0
    private var end: CFAbsoluteTime = 0
    
    @objc public var preds: MLMultiArray?
    @objc public var protos: MLMultiArray?

    let initLock = NSLock()
    @objc public var Init: Bool {
        get { initLock.sync { predictionRequest != nil} }
    }
    
    private var success: Bool = false
    let successLock = NSLock()
    @objc public var Success: Bool {
        get { successLock.sync { success } }
        set(value) { successLock.sync { success = value } }
    }
    
    private var detected: Bool = false
    let detectLock = NSLock()
    @objc public var Detected: Bool {
        get { detectLock.sync { detected } }
        set(value) { detectLock.sync { detected = value } }
    }
    
    private var predictionRequest:VNCoreMLRequest? = nil
    
    override init() {
        super.init()
        
        visionQueue.async {
            do {
                let config = MLModelConfiguration()
                config.computeUnits = .all
                
                let model = try VNCoreMLModel(for: FastSAM_s(configuration: config).model)
                
                self.predictionRequest = VNCoreMLRequest(model: model, completionHandler: self.detectionCompleteHandler)
                
                self.predictionRequest?.imageCropAndScaleOption = VNImageCropAndScaleOption.scaleFill
                
            } catch {
                fatalError("can't load Vision ML model: \(error)")
            }
        }
    }

    @objc public func startDetection(buffer: CVPixelBuffer, orientation: CGImagePropertyOrientation) -> Bool {
        start = CFAbsoluteTimeGetCurrent()
        self.detected = false
        self.retainedBuffer = buffer
        
        let imageRequestHandler = VNImageRequestHandler(cvPixelBuffer: self.retainedBuffer!, orientation: orientation)
        
        visionQueue.async {
            do {
                defer { self.retainedBuffer = nil }
                try imageRequestHandler.perform([self.predictionRequest!])
            } catch {
                fatalError("Perform Failed:\"\(error)\"")
            }
        }
        
        return true
    }
    
    private func detectionCompleteHandler(request: VNRequest, error: Error?) {
        defer {
            self.detected = true
        }
        
        guard error == nil else {
            self.Success = false
            return
        }
        end = CFAbsoluteTimeGetCurrent()
        print("model inference time ", end - start)
        
        if let predictions = request.results as?
            [VNCoreMLFeatureValueObservation] {
            preds = predictions[0].featureValue.multiArrayValue
            protos = predictions[1].featureValue.multiArrayValue
            self.Success = true
        } else {
            self.Success = false
        }

        return
    }
    
    @objc public func reset() {
        preds = nil
        protos = nil
    }
}
