//
//  array_utils.swift
//  Unity-iPhone
//
//  Created by 박종현 on 4/16/24.
//

import Foundation
import CoreML


@available(iOS 16.0, *)
func copyMLMultiArray2Array(mlMultiArray: MLMultiArray) -> Array<Float32> {
    if mlMultiArray.shape.count == 3 {
        return copyDim2(mlMultiArray: mlMultiArray)
    }
    else if mlMultiArray.shape.count == 4 {
        return copyDim3(mlMultiArray: mlMultiArray)
    }
    
    return Array<Float32>()
}

@available(iOS 16.0, *)
func copyDim2(mlMultiArray: MLMultiArray) -> Array<Float32> {
    let shapes = mlMultiArray.shape
    let strides = mlMultiArray.strides
    var array = [Float32](repeating: 0, count: shapes[1].intValue * shapes[2].intValue)
    
    mlMultiArray.withUnsafeBytes { (bufferPointer: UnsafeRawBufferPointer) in
        if let mlArrayPointer = bufferPointer.bindMemory(to: Float32.self).baseAddress {
            let diff = (strides[1].intValue - shapes[2].intValue)
            let col = shapes[2].intValue
            
            for i in 0 ..< shapes[1].intValue {
                for j in 0 ..< col {
                    let idx = i * col + j
                    array[idx] = mlArrayPointer[idx + i * diff]
                }
            }
        }
    }
    return array
}

@available(iOS 16.0, *)
func copyDim3(mlMultiArray: MLMultiArray) -> Array<Float32> {
    let shapes = mlMultiArray.shape
    let strides = mlMultiArray.strides
    var array = [Float32](repeating: 0, count: shapes[1].intValue * shapes[2].intValue * shapes[3].intValue)
    
    mlMultiArray.withUnsafeBytes { (bufferPointer: UnsafeRawBufferPointer) in
        if let mlArrayPointer = bufferPointer.bindMemory(to: Float32.self).baseAddress {
            let diff = (strides[2].intValue - shapes[3].intValue)
            let dim1 = shapes[1].intValue
            let dim2 = shapes[2].intValue
            let dim3 = shapes[3].intValue
            
            for i in 0 ..< dim1 {
                for j in 0 ..< dim2 {
                    for k in 0 ..< dim3 {
                        let idx = i * dim2 * dim3 + j * dim3 + k
                        array[idx] = mlArrayPointer[idx + (i * dim2 + j) * diff]
                    }
                }
            }
        }
    }
    return array
}
