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

@available(iOS 15.0, *)
@objc public class FastSAM : NSObject {
    
    @objc public static let shared = FastSAM()
    
    private let visionQueue = DispatchQueue(label: "com.pjh.FastSAM")
    
    private var retainedBuffer: CVPixelBuffer?
    
    @objc public var protoBuffer: [Float32] = []
    @objc public var predBuffer: [Float32] = []
    
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

        if let predictions = request.results as?
            [VNCoreMLFeatureValueObservation] {
            let preds = predictions[0].featureValue.multiArrayValue
            let protos = predictions[1].featureValue.multiArrayValue
            
            let predsLength = preds?.count
            let protosLength = protos?.count
            
            for i in 0...predsLength! - 1 {
                predBuffer.append(preds![i].floatValue)
            }
            for i in 0...protosLength! - 1 {
                protoBuffer.append(protos![i].floatValue)
            }

            self.Success = true
        } else {
            self.Success = false
        }

        return
    }
    
    @objc public func reset() {
        protoBuffer.removeAll()
        predBuffer.removeAll()
    }
}
