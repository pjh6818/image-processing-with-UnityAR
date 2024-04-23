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

    [SerializeField]
    RawImage m_MaskImage;

    [SerializeField]
    Button m_FastSAMProcessButton;

    [SerializeField]
    GameObject pointPrefab;

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Button m_AddPointButton;

    [SerializeField]
    Button m_DeletePointButton;

    [SerializeField]
    ShineEffect shineEffect;

    [SerializeField]
    RawImage m_CaptureImage;

    [SerializeField]
    Button m_BackButton;

    void OnEnable()
    {
        m_AddPointButton?.onClick.AddListener(OnClickAddPointButton);
        m_DeletePointButton?.onClick.AddListener(OnClickDeletePointButton);
        m_FastSAMProcessButton?.onClick.AddListener(OnClickFastSAMProcessButton);
        m_BackButton?.onClick.AddListener(OnClickBackButton);

        if (fastSAM == null)
            fastSAM = GetComponent<FastSAM>();

        if (fastSAM != null)
            fastSAM.OnRequestDone += OnFastSAMDone;
    }

    void OnDisable()
    {
        m_AddPointButton?.onClick.RemoveListener(OnClickAddPointButton);
        m_DeletePointButton?.onClick.RemoveListener(OnClickDeletePointButton);
        m_FastSAMProcessButton.onClick.RemoveListener(OnClickFastSAMProcessButton);
        m_BackButton?.onClick.RemoveListener(OnClickBackButton);

        if (fastSAM != null)
            fastSAM.OnRequestDone -= OnFastSAMDone;
    }

    void OnClickAddPointButton()
    {
        var point = Instantiate(pointPrefab, canvas.transform);
        pointMarkers.Add(point);
    }

    void OnClickDeletePointButton()
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

        byte[] image = m_ArCameraImage.GetRGBFromRenderTexture(out int width, out int height);
        UpdateTexture(ref m_RGBTexture, image, width, height, TextureFormat.RGB24);

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
        {
            UpdateTexture(ref m_MaskTexture, mask.Item1, mask.Item2, mask.Item3, TextureFormat.R8);
            m_MaskImage.texture = m_MaskTexture;

            ShowCaptureImage();
        }
    }

    void ShowCaptureImage()
    {
        // m_MaskImage.gameObject.SetActive(false);
        m_AddPointButton.gameObject.SetActive(false);
        m_DeletePointButton.gameObject.SetActive(false);
        m_FastSAMProcessButton.gameObject.SetActive(false);

        foreach (var obj in pointMarkers)
            obj.SetActive(false);
        
        m_CaptureImage.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        shineEffect.gameObject.SetActive(true);

        m_CaptureImage.texture = m_RGBTexture;
        shineEffect.LoadMaskTexture(m_MaskTexture);
        shineEffect.startShine();
    }

    public void OnClickBackButton()
    {
        shineEffect.stopShine();

        m_CaptureImage.gameObject.SetActive(false);
        m_BackButton.gameObject.SetActive(false);
        shineEffect.gameObject.SetActive(false);

        foreach (var obj in pointMarkers)
            obj.SetActive(true);

        // m_MaskImage.gameObject.SetActive(true);
        m_AddPointButton.gameObject.SetActive(true);
        m_DeletePointButton.gameObject.SetActive(true);
        m_FastSAMProcessButton.gameObject.SetActive(true);
    }

    void UpdateTexture(ref Texture2D tex, byte[] buf, int width, int height, TextureFormat textureFormat)
    {
        if (tex == null)
            tex = new Texture2D(width, height, textureFormat, false);
        
        if (buf == null)
            return;

        tex.LoadRawTextureData(buf);
        tex.Apply();
    }

    FastSAM fastSAM;

    List<GameObject> pointMarkers = new List<GameObject>();
    Texture2D m_MaskTexture;
    Texture2D m_RGBTexture;

    Stopwatch watch = new Stopwatch();
}
