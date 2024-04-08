using System;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class FastSAM : MonoBehaviour
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern bool requestFastSAM(IntPtr rgb, int width, int height, bool with_alpha);

    [DllImport("__Internal")]
    private static extern bool isFastSAMDone();

    [DllImport("__Internal")]
    private static extern bool getFastSAMResult([In, Out] float[] preds, [In, Out] float[] protos);

    [DllImport("__Internal")]
    private static extern IntPtr FastSAMWithPoints(int img_width, int img_height, 
                        IntPtr preds, IntPtr protos, Vector2Int[] points, int[] points_label, int point_size);
#endif

    public event EventHandler<IntPtr> OnRequestDone;

    private float[] preds = new float[37*6300];
    private float[] protos = new float[32*120*160];


    private bool detecting = false;

    public void RequestAsync(IntPtr rgb, int width, int height, bool withAlpha, Vector2 point)
    {
        if (detecting)
            return;

        if (rgb == IntPtr.Zero)
            return;
        
        StartCoroutine(Request(rgb, width, height, withAlpha, point));
    }

    IEnumerator Request(IntPtr bufPtr, int width, int height, bool withAlpha, Vector2 point)
    {
        detecting = true;

#if UNITY_IOS
        requestFastSAM(bufPtr, width, height, withAlpha);

        yield return new WaitUntil(() => isFastSAMDone());

        bool success = getFastSAMResult(preds, protos);
        IntPtr maskPtr = IntPtr.Zero;

        if (success)
            maskPtr = PointPrompts(width, height, point);
            
        OnRequestDone?.Invoke(this, maskPtr);
        Marshal.FreeHGlobal(maskPtr);
#endif

        detecting = false;
    }

    IntPtr PointPrompts(int width, int height, Vector2 point)
    {
        IntPtr predsPtr = IntPtr.Zero, protosPtr = IntPtr.Zero, maskPtr = IntPtr.Zero;
        Vector2Int[] pts = new Vector2Int[1];
        int[] ptLabels = new int[1];
        pts[0] = new Vector2Int((int)(point.x * 480), (int)(point.y * 640));
        ptLabels[0] = 1;

        unsafe {
            fixed (float* p = preds) { predsPtr = (IntPtr)p; }
            fixed (float* p = protos) { protosPtr = (IntPtr)p; }
        }
        
        return FastSAMWithPoints(480, 640, predsPtr, protosPtr, pts, ptLabels, 1);
    }
}
