using System;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

public class EdgeDetector
{
#if UNITY_IOS	
    [DllImport("__Internal")]
    private static extern IntPtr detectEdge(IntPtr rgb, int width, int height, ref int o_width, ref int o_height);
#endif
    
    static public byte[] detect(byte[] image, int width, int height, ref int edge_width, ref int edge_height)
    {
        byte[] edge = null;

        if (image == null)
            return edge;

        IntPtr imagePtr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* p = image) { imagePtr = (IntPtr)p; }
        }
        
#if UNITY_IOS
        IntPtr processed = detectEdge(imagePtr, width, height, ref edge_width, ref edge_height);
        edge = new byte[edge_width * edge_height];
        Marshal.Copy(processed, edge, 0, edge_width * edge_height);
        Marshal.FreeHGlobal(processed);
#endif

        return edge;
    }
}
