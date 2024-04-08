using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Unity.VisualScripting;


public class ImageProcessor : MonoBehaviour
{
#if UNITY_IOS	
    [DllImport("__Internal")]
    private static extern IntPtr detectEdge(IntPtr rgb, int width, int height, ref int o_width, ref int o_height);
#endif


    [SerializeField]
    ARCameraImage m_ArCameraImage;

    public ARCameraImage arCameraImage
    {
        get => m_ArCameraImage;
        set => m_ArCameraImage = value;
    }

    [SerializeField]
    RawImage m_EdgeImage;

    public RawImage edgeImage
    {
        get => m_EdgeImage;
        set => m_EdgeImage = value;
    }

    [SerializeField]
    RawImage m_MaskImage;

    public RawImage maskImage
    {
        get => m_MaskImage;
        set => m_MaskImage = value;
    }

    [SerializeField]
    Button m_EdgeProcessButton;

    public Button edgeProcessButton
    {
        get => m_EdgeProcessButton;
        set => m_EdgeProcessButton = value;
    }

    [SerializeField]
    Button m_FastSAMProcessButton;

    public Button FastSAMProcessButton
    {
        get => m_FastSAMProcessButton;
        set => m_FastSAMProcessButton = value;
    }

    FastSAM fastSAM;

    void OnEnable()
    {
        if (m_EdgeProcessButton != null)
            m_EdgeProcessButton.onClick.AddListener(OnClickEdgeProcessButton);

        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.AddListener(OnClickFastSAMProcessButton);

        if (fastSAM == null)
            fastSAM = GetComponent<FastSAM>();
        
        if (fastSAM != null)
            fastSAM.OnRequestDone += OnFastSAMDone;
    }

    void OnDisable()
    {
        if (m_EdgeProcessButton != null)
            m_EdgeProcessButton.onClick.RemoveListener(OnClickEdgeProcessButton);

        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.RemoveListener(OnClickFastSAMProcessButton);
        
        if (fastSAM != null)
            fastSAM.OnRequestDone -= OnFastSAMDone;
    }

    void UpdateRawImage(ref Texture2D tex, ref RawImage image, IntPtr buf, int width, int height)
    {
        if (tex == null)
            tex = new Texture2D(width, height, TextureFormat.R8, false);
        
        tex.LoadRawTextureData(buf, width * height);
        tex.Apply();

        image.texture = tex;
    }

    void OnClickEdgeProcessButton()
    {
    #if UNITY_IOS
        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);

        if (image == null)
            return;

        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        int o_width=0, o_height=0;

        IntPtr processed = detectEdge(imagePtr, width, height, ref o_width, ref o_height);

        UpdateRawImage(ref m_EdgeTexture, ref m_EdgeImage, processed, o_width, o_height);

        Marshal.FreeHGlobal(processed);
    #endif
    }

    void OnClickFastSAMProcessButton()
    {
        if (fastSAM == null)
            return;

        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);
        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        fastSAM.RequestAsync(imagePtr, width, height, true, new Vector2(0.5f, 0.5f));
    }

    void OnFastSAMDone(object sender, IntPtr maskPtr)
    {
        if (maskPtr == IntPtr.Zero)
            return;
        
        UpdateRawImage(ref m_MaskTexture, ref m_MaskImage, maskPtr, 480, 640);
    }

    Texture2D m_EdgeTexture;
    Texture2D m_MaskTexture;
}
