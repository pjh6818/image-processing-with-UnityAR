using System;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public class FastSAMInARSample : MonoBehaviour
{
    [SerializeField]
    Camera m_Camera;

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

    [SerializeField]
    GameObject pointPrefab;

    [SerializeField]
    Canvas canvas;

    public RawImage tempImage;

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

    public void OnClickAddPointButton()
    {
        var point = Instantiate(pointPrefab, canvas.transform);
        pointMarkers.Add(point);
    }

    public void OnClickDeletePointButton()
    {
        foreach (var obj in pointMarkers)
        {
            Destroy(obj);
        }
        pointMarkers.Clear();
    }

    void OnClickFastSAMProcessButton()
    {
        if (fastSAM == null)
            return;

        // UpdateRawImage(ref tempTex, ref tempImage, m_ArCameraImage.GetRGBFromRenderTexture(), Screen.width / 4, Screen.height / 4, TextureFormat.RGB24);

        byte[] image = m_ArCameraImage.GetRGBFromRenderTexture(out int width, out int height);
        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        var points = pointMarkers.Select(o => { 
            var rectTransform = o.GetComponent<RectTransform>();
            return new Vector2(rectTransform.position.x / Screen.width, 1.0f - (rectTransform.position.y / Screen.height));
            }).ToArray();

        watch.Start();
        fastSAM.RequestAsync(imagePtr, width, height, false, points);
    }

    void OnFastSAMDone(object sender, (byte[], int, int) mask)
    {
        watch.Stop();
        UnityEngine.Debug.Log("FastSAM entire time elapsed : " + watch.ElapsedMilliseconds + "ms");
        watch.Reset();

        if (mask.Item1 != null)
            UpdateRawImage(ref m_MaskTexture, ref m_MaskImage, mask.Item1, mask.Item2, mask.Item3, TextureFormat.R8);
    }

    void UpdateRawImage(ref Texture2D tex, ref RawImage image, byte[] buf, int width, int height, TextureFormat textureFormat)
    {
        if (tex == null)
            tex = new Texture2D(width, height, textureFormat, false);
        
        tex.LoadRawTextureData(buf);
        tex.Apply();

        image.texture = tex;
    }

    FastSAM fastSAM;

    List<GameObject> pointMarkers = new List<GameObject>();
    Texture2D m_MaskTexture;
    Texture2D tempTex;
    Stopwatch watch = new Stopwatch();
}
