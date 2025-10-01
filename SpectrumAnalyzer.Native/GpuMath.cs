using System.Numerics;
using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Native;

public static class GpuMath
{
    private const string Lib = "gpu_math.so";
    
    // factor must be power of 2. Output is size of dataLen / 2.
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void decimate_data(IntPtr iqData, int dataLen, int factor, UIntPtr output);
    
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fft(IntPtr complexData, int dataLen, int sampleRate,  UIntPtr output);

    
}