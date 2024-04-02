using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;


public class ImageProcessor : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern IntPtr processImage(IntPtr rgb, int width, int height, ref int o_width, ref int o_height);
    
    [SerializeField]
    ARCameraImage m_ArCameraImage;

    public ARCameraImage arCameraImage
    {
        get => m_ArCameraImage;
        set => m_ArCameraImage = value;
    }

    [SerializeField]
    RawImage m_RawCameraImage;

    public RawImage rawCameraImage
    {
        get => m_RawCameraImage;
        set => m_RawCameraImage = value;
    }

    [SerializeField]
    Button m_ProcessButton;

    public Button processButton
    {
        get => m_ProcessButton;
        set => m_ProcessButton = value;
    }


    void OnEnable()
    {
        if (m_ProcessButton != null)
        {
            m_ProcessButton.onClick.AddListener(OnClickProcessButton);
        }
    }

    void OnDisable()
    {
        if (m_ProcessButton != null)
        {
            m_ProcessButton.onClick.RemoveListener(OnClickProcessButton);
        }
    }

    void UpdateRawImage(IntPtr processed, int width, int height)
    {
        if (m_ProcessedTexture == null)
        {
            m_ProcessedTexture = new Texture2D(width, height, TextureFormat.R8, false);
        }

        m_ProcessedTexture.LoadRawTextureData(processed, width * height);
        m_ProcessedTexture.Apply();

        m_RawCameraImage.texture = m_ProcessedTexture;
    }

    void OnClickProcessButton()
    {
        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);

        if (image == null)
            return;

        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        int o_width=0, o_height=0;

        IntPtr processed = processImage(imagePtr, width, height, ref o_width, ref o_height);

        UpdateRawImage(processed, o_width, o_height);

        Marshal.FreeHGlobal(processed);
    }

    Texture2D m_ProcessedTexture;
}
