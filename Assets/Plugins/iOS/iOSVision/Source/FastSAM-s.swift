//
// FastSAM_s.swift
//
// This file was automatically generated and should not be edited.
//

import CoreML


/// Model Prediction Input Type
@available(macOS 12.0, iOS 15.0, tvOS 15.0, watchOS 8.0, *)
class FastSAM_sInput : MLFeatureProvider {

    /// image as color (kCVPixelFormatType_32BGRA) image buffer, 640 pixels wide by 480 pixels high
    var image: CVPixelBuffer

    var featureNames: Set<String> {
        get {
            return ["image"]
        }
    }
    
    func featureValue(for featureName: String) -> MLFeatureValue? {
        if (featureName == "image") {
            return MLFeatureValue(pixelBuffer: image)
        }
        return nil
    }
    
    init(image: CVPixelBuffer) {
        self.image = image
    }

    convenience init(imageWith image: CGImage) throws {
        self.init(image: try MLFeatureValue(cgImage: image, pixelsWide: 640, pixelsHigh: 480, pixelFormatType: kCVPixelFormatType_32ARGB, options: nil).imageBufferValue!)
    }

    convenience init(imageAt image: URL) throws {
        self.init(image: try MLFeatureValue(imageAt: image, pixelsWide: 640, pixelsHigh: 480, pixelFormatType: kCVPixelFormatType_32ARGB, options: nil).imageBufferValue!)
    }

    func setImage(with image: CGImage) throws  {
        self.image = try MLFeatureValue(cgImage: image, pixelsWide: 640, pixelsHigh: 480, pixelFormatType: kCVPixelFormatType_32ARGB, options: nil).imageBufferValue!
    }

    func setImage(with image: URL) throws  {
        self.image = try MLFeatureValue(imageAt: image, pixelsWide: 640, pixelsHigh: 480, pixelFormatType: kCVPixelFormatType_32ARGB, options: nil).imageBufferValue!
    }

}


/// Model Prediction Output Type
@available(macOS 12.0, iOS 15.0, tvOS 15.0, watchOS 8.0, *)
class FastSAM_sOutput : MLFeatureProvider {

    /// Source provided by CoreML
    private let provider : MLFeatureProvider

    /// var_1098 as 1 × 37 × 6300 3-dimensional array of floats
    var var_1098: MLMultiArray {
        return self.provider.featureValue(for: "var_1098")!.multiArrayValue!
    }

    /// var_1098 as 1 × 37 × 6300 3-dimensional array of floats
    var var_1098ShapedArray: MLShapedArray<Float> {
        return MLShapedArray<Float>(self.var_1098)
    }

    /// p as 1 × 32 × 120 × 160 4-dimensional array of floats
    var p: MLMultiArray {
        return self.provider.featureValue(for: "p")!.multiArrayValue!
    }

    /// p as 1 × 32 × 120 × 160 4-dimensional array of floats
    var pShapedArray: MLShapedArray<Float> {
        return MLShapedArray<Float>(self.p)
    }

    var featureNames: Set<String> {
        return self.provider.featureNames
    }
    
    func featureValue(for featureName: String) -> MLFeatureValue? {
        return self.provider.featureValue(for: featureName)
    }

    init(var_1098: MLMultiArray, p: MLMultiArray) {
        self.provider = try! MLDictionaryFeatureProvider(dictionary: ["var_1098" : MLFeatureValue(multiArray: var_1098), "p" : MLFeatureValue(multiArray: p)])
    }

    init(features: MLFeatureProvider) {
        self.provider = features
    }
}


/// Class for model loading and prediction
@available(macOS 12.0, iOS 15.0, tvOS 15.0, watchOS 8.0, *)
class FastSAM_s {
    let model: MLModel

    /// URL of model assuming it was installed in the same bundle as this class
    class var urlOfModelInThisBundle : URL {
        let bundle = Bundle(for: self)
        return bundle.url(forResource: "FastSAM-s", withExtension:"mlmodelc")!
    }

    /**
        Construct FastSAM_s instance with an existing MLModel object.

        Usually the application does not use this initializer unless it makes a subclass of FastSAM_s.
        Such application may want to use `MLModel(contentsOfURL:configuration:)` and `FastSAM_s.urlOfModelInThisBundle` to create a MLModel object to pass-in.

        - parameters:
          - model: MLModel object
    */
    init(model: MLModel) {
        self.model = model
    }

    /**
        Construct a model with configuration

        - parameters:
           - configuration: the desired model configuration

        - throws: an NSError object that describes the problem
    */
    convenience init(configuration: MLModelConfiguration = MLModelConfiguration()) throws {
        try self.init(contentsOf: type(of:self).urlOfModelInThisBundle, configuration: configuration)
    }

    /**
        Construct FastSAM_s instance with explicit path to mlmodelc file
        - parameters:
           - modelURL: the file url of the model

        - throws: an NSError object that describes the problem
    */
    convenience init(contentsOf modelURL: URL) throws {
        try self.init(model: MLModel(contentsOf: modelURL))
    }

    /**
        Construct a model with URL of the .mlmodelc directory and configuration

        - parameters:
           - modelURL: the file url of the model
           - configuration: the desired model configuration

        - throws: an NSError object that describes the problem
    */
    convenience init(contentsOf modelURL: URL, configuration: MLModelConfiguration) throws {
        try self.init(model: MLModel(contentsOf: modelURL, configuration: configuration))
    }

    /**
        Construct FastSAM_s instance asynchronously with optional configuration.

        Model loading may take time when the model content is not immediately available (e.g. encrypted model). Use this factory method especially when the caller is on the main thread.

        - parameters:
          - configuration: the desired model configuration
          - handler: the completion handler to be called when the model loading completes successfully or unsuccessfully
    */
    class func load(configuration: MLModelConfiguration = MLModelConfiguration(), completionHandler handler: @escaping (Swift.Result<FastSAM_s, Error>) -> Void) {
        return self.load(contentsOf: self.urlOfModelInThisBundle, configuration: configuration, completionHandler: handler)
    }

    /**
        Construct FastSAM_s instance asynchronously with optional configuration.

        Model loading may take time when the model content is not immediately available (e.g. encrypted model). Use this factory method especially when the caller is on the main thread.

        - parameters:
          - configuration: the desired model configuration
    */
    class func load(configuration: MLModelConfiguration = MLModelConfiguration()) async throws -> FastSAM_s {
        return try await self.load(contentsOf: self.urlOfModelInThisBundle, configuration: configuration)
    }

    /**
        Construct FastSAM_s instance asynchronously with URL of the .mlmodelc directory with optional configuration.

        Model loading may take time when the model content is not immediately available (e.g. encrypted model). Use this factory method especially when the caller is on the main thread.

        - parameters:
          - modelURL: the URL to the model
          - configuration: the desired model configuration
          - handler: the completion handler to be called when the model loading completes successfully or unsuccessfully
    */
    class func load(contentsOf modelURL: URL, configuration: MLModelConfiguration = MLModelConfiguration(), completionHandler handler: @escaping (Swift.Result<FastSAM_s, Error>) -> Void) {
        MLModel.load(contentsOf: modelURL, configuration: configuration) { result in
            switch result {
            case .failure(let error):
                handler(.failure(error))
            case .success(let model):
                handler(.success(FastSAM_s(model: model)))
            }
        }
    }

    /**
        Construct FastSAM_s instance asynchronously with URL of the .mlmodelc directory with optional configuration.

        Model loading may take time when the model content is not immediately available (e.g. encrypted model). Use this factory method especially when the caller is on the main thread.

        - parameters:
          - modelURL: the URL to the model
          - configuration: the desired model configuration
    */
    class func load(contentsOf modelURL: URL, configuration: MLModelConfiguration = MLModelConfiguration()) async throws -> FastSAM_s {
        let model = try await MLModel.load(contentsOf: modelURL, configuration: configuration)
        return FastSAM_s(model: model)
    }

    /**
        Make a prediction using the structured interface

        - parameters:
           - input: the input to the prediction as FastSAM_sInput

        - throws: an NSError object that describes the problem

        - returns: the result of the prediction as FastSAM_sOutput
    */
    func prediction(input: FastSAM_sInput) throws -> FastSAM_sOutput {
        return try self.prediction(input: input, options: MLPredictionOptions())
    }

    /**
        Make a prediction using the structured interface

        - parameters:
           - input: the input to the prediction as FastSAM_sInput
           - options: prediction options 

        - throws: an NSError object that describes the problem

        - returns: the result of the prediction as FastSAM_sOutput
    */
    func prediction(input: FastSAM_sInput, options: MLPredictionOptions) throws -> FastSAM_sOutput {
        let outFeatures = try model.prediction(from: input, options:options)
        return FastSAM_sOutput(features: outFeatures)
    }

    /**
        Make an asynchronous prediction using the structured interface

        - parameters:
           - input: the input to the prediction as FastSAM_sInput
           - options: prediction options 

        - throws: an NSError object that describes the problem

        - returns: the result of the prediction as FastSAM_sOutput
    */
    @available(macOS 14.0, iOS 17.0, tvOS 17.0, watchOS 10.0, *)
    func prediction(input: FastSAM_sInput, options: MLPredictionOptions = MLPredictionOptions()) async throws -> FastSAM_sOutput {
        let outFeatures = try await model.prediction(from: input, options:options)
        return FastSAM_sOutput(features: outFeatures)
    }

    /**
        Make a prediction using the convenience interface

        - parameters:
            - image as color (kCVPixelFormatType_32BGRA) image buffer, 640 pixels wide by 480 pixels high

        - throws: an NSError object that describes the problem

        - returns: the result of the prediction as FastSAM_sOutput
    */
    func prediction(image: CVPixelBuffer) throws -> FastSAM_sOutput {
        let input_ = FastSAM_sInput(image: image)
        return try self.prediction(input: input_)
    }

    /**
        Make a batch prediction using the structured interface

        - parameters:
           - inputs: the inputs to the prediction as [FastSAM_sInput]
           - options: prediction options 

        - throws: an NSError object that describes the problem

        - returns: the result of the prediction as [FastSAM_sOutput]
    */
    func predictions(inputs: [FastSAM_sInput], options: MLPredictionOptions = MLPredictionOptions()) throws -> [FastSAM_sOutput] {
        let batchIn = MLArrayBatchProvider(array: inputs)
        let batchOut = try model.predictions(from: batchIn, options: options)
        var results : [FastSAM_sOutput] = []
        results.reserveCapacity(inputs.count)
        for i in 0..<batchOut.count {
            let outProvider = batchOut.features(at: i)
            let result =  FastSAM_sOutput(features: outProvider)
            results.append(result)
        }
        return results
    }
}
