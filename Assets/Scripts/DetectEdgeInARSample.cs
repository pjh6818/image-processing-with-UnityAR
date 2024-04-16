using System;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;


public class DetectEdgeInARSample : MonoBehaviour
{
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
    Button m_EdgeDetectButton;

    public Button edgeDetectButton
    {
        get => m_EdgeDetectButton;
        set => m_EdgeDetectButton = value;
    }

    void OnEnable()
    {
        if (m_EdgeDetectButton != null)
            m_EdgeDetectButton.onClick.AddListener(OnClickEdgeDetectButton);
    }

    void OnDisable()
    {
        if (m_EdgeDetectButton != null)
            m_EdgeDetectButton.onClick.RemoveListener(OnClickEdgeDetectButton);
    }

    public byte[] detectEdge(out int edge_width, out int edge_height)
    {
        edge_width = edge_height = 0;

        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);
        byte[] edge = EdgeDetector.detect(image, width, height, ref edge_width, ref edge_height);

        return edge;
    }


    void OnClickEdgeDetectButton()
    {
        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);

        int edge_width=0, edge_height=0;
        byte[] edge = EdgeDetector.detect(image, width, height, ref edge_width, ref edge_height);

        if (edge != null)
            UpdateRawImage(ref m_EdgeTexture, ref m_EdgeImage, edge, edge_width, edge_height);
    }

    void UpdateRawImage(ref Texture2D tex, ref RawImage image, byte[] buf, int width, int height)
    {
        if (tex == null)
            tex = new Texture2D(width, height, TextureFormat.R8, false);
        
        tex.LoadRawTextureData(buf);
        tex.Apply();

        image.texture = tex;
    }

    Texture2D m_EdgeTexture;
}
