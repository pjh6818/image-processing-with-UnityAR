using System;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
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

    [SerializeField]
    ImageProcessor imageProcessor;

    void OnEnable()
    {
        if (m_EdgeProcessButton != null)
            m_EdgeProcessButton.onClick.AddListener(OnClickEdgeProcessButton);

        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.AddListener(OnClickFastSAMProcessButton);

        if (imageProcessor != null)
            imageProcessor.AddOnFastSAMDoneListener(OnFastSAMDone);
    }

    void OnDisable()
    {
        if (m_EdgeProcessButton != null)
            m_EdgeProcessButton.onClick.RemoveListener(OnClickEdgeProcessButton);

        if (m_FastSAMProcessButton != null)
            m_FastSAMProcessButton.onClick.RemoveListener(OnClickFastSAMProcessButton);
        
        if (imageProcessor != null)
            imageProcessor.RemoveFastSAMDoneListener(OnFastSAMDone);
    }

    void UpdateRawImage(ref Texture2D tex, ref RawImage image, byte[] buf, int width, int height)
    {
        if (tex == null)
            tex = new Texture2D(width, height, TextureFormat.R8, false);
        
        tex.LoadRawTextureData(buf);
        tex.Apply();

        image.texture = tex;
    }

    void OnClickEdgeProcessButton()
    {
        byte[] edge = imageProcessor.detectEdge(out int edge_width, out int edge_height);

        if (edge != null)
            UpdateRawImage(ref m_EdgeTexture, ref m_EdgeImage, edge, edge_width, edge_height);
    }

    void OnClickFastSAMProcessButton()
    {
        imageProcessor.doFastSAM(new Vector2(0.5f, 0.5f));
    }

    void OnFastSAMDone(object sender, (byte[], int, int) mask)
    {
        if (mask.Item1 != null)
            UpdateRawImage(ref m_MaskTexture, ref m_MaskImage, mask.Item1, mask.Item2, mask.Item3);
    }

    Texture2D m_EdgeTexture;
    Texture2D m_MaskTexture;
}
