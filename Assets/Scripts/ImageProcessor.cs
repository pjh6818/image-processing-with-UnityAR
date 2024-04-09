using System;
using UnityEngine;
using System.Runtime.InteropServices;


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

    FastSAM fastSAM;

    public void AddOnFastSAMDoneListener(EventHandler<(byte[], int, int)> func)
    {
        if (fastSAM == null)
            fastSAM = GetComponent<FastSAM>();

        if (fastSAM != null)
            fastSAM.OnRequestDone += func;
    }

    public void RemoveFastSAMDoneListener(EventHandler<(byte[], int, int)> func)
    {
        if (fastSAM != null)
            fastSAM.OnRequestDone -= func;
    }

    public byte[] detectEdge(out int edge_width, out int edge_height)
    {
        byte[] edge = null;
        edge_width = edge_height = 0;

#if UNITY_IOS
        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);

        if (image == null)
            return null;

        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        IntPtr processed = detectEdge(imagePtr, width, height, ref edge_width, ref edge_height);
        edge = new byte[edge_width * edge_height];
        Marshal.Copy(processed, edge, 0, edge_width * edge_height);
        Marshal.FreeHGlobal(processed);
#endif

        return edge;
    }

    public void doFastSAM(Vector2 point)
    {
        if (fastSAM == null)
            return;

        byte[] image = m_ArCameraImage.GetRGB(out int width, out int height);
        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }

        fastSAM.RequestAsync(imagePtr, width, height, true, point);
    }
}
