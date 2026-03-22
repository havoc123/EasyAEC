using System.Runtime.InteropServices;

namespace EasyAEC_GUI;

/// <summary>
/// 原生 AEC_Core.dll 的 P/Invoke 封装（Mock 实现）。
/// </summary>
internal static class AecWrapper
{
    private const string DllName = "AEC_Core.dll";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int AEC_Init(int sampleRate, int suppressionLevel);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void AEC_ProcessFrame(
        [In] float[] nearEndMic,
        [In] float[] farEndRef,
        [Out] float[] outClean,
        int frameLength);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void AEC_Destroy();
}
