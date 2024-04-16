using System;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class FastSAMInARSample : MonoBehaviour
{
    [SerializeField]
    ARCameraImage m_ArCameraImage;

    public ARCameraImage arCameraImage
    {
        get => m_ArCameraImage;
        set => m_ArCameraImage = value;
    }

    [SerializeField]
    RawImage m_MaskImage;

    public RawImage maskImage
    {
        get => m_MaskImage;
        set => m_MaskImage = value;
    }

    [SerializeField]
    Button m_FastSAMProcessButton;

    public Button FastSAMProcessButton
    {
        get => m_FastSAMProcessButton;
        set => m_FastSAMProcessButton = value;
    }

    void OnEnable()
    {
        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.AddListener(OnClickFastSAMProcessButton);

        if (fastSAM == null)
            fastSAM = GetComponent<FastSAM>();

        if (fastSAM != null)
            fastSAM.OnRequestDone += OnFastSAMDone;
    }

    void OnDisable()
    {
        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.RemoveListener(OnClickFastSAMProcessButton);

        if (fastSAM != null)
            fastSAM.OnRequestDone -= OnFastSAMDone;
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
        points = new Vector2[1];
        points[0] = new Vector2(0.5f, 0.5f);
        watch.Start();
        fastSAM.RequestAsync(imagePtr, width, height, true, points);
    }

    void OnFastSAMDone(object sender, (byte[], int, int) mask)
    {
        watch.Stop();
        UnityEngine.Debug.Log("FastSAM entire time elapsed : " + watch.ElapsedMilliseconds + "ms");
        watch.Reset();
        
        if (mask.Item1 != null)
            UpdateRawImage(ref m_MaskTexture, ref m_MaskImage, mask.Item1, mask.Item2, mask.Item3);
    }

    void UpdateRawImage(ref Texture2D tex, ref RawImage image, byte[] buf, int width, int height)
    {
        if (tex == null)
            tex = new Texture2D(width, height, TextureFormat.R8, false);
        
        tex.LoadRawTextureData(buf);
        tex.Apply();

        image.texture = tex;
    }

    FastSAM fastSAM;
    Vector2[] points;
    Texture2D m_MaskTexture;

    Stopwatch watch = new Stopwatch();
}
