using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCameraImage : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events.")]
    ARCameraManager m_CameraManager;

    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set => m_CameraManager = value;
    }

    [SerializeField]
    ARCameraBackground m_CameraBackground;

    public ARCameraBackground cameraBackground
    {
        get => m_CameraBackground;
        set => m_CameraBackground = value;
    }

    public byte[] GetRGBFromRenderTexture(out int width, out int height)
    {
        width = Screen.width;
        height = Screen.height;

        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24);
        Graphics.Blit(null, renderTexture, m_CameraBackground.material);

        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // Read the pixels from the RenderTexture
        var temp = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = temp;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D.GetRawTextureData();
    }


    public unsafe byte[] GetRGB(out int width, out int height)
    {
        width = height = -1;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return null;
        }

        var format = TextureFormat.RGBA32;

        if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
        {
            m_CameraTexture = new Texture2D(image.width, image.height, format, false);
        }

        var conversionParams = new XRCpuImage.ConversionParams(image, format, m_Transformation);

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
        try
        {
            image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        m_CameraTexture.Apply();

        width = image.width;
        height = image.height;

        return rawTextureData.ToArray();
    }

    XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorY;
    Texture2D m_CameraTexture;
}
