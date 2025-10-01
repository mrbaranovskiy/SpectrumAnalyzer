using System.Numerics;
using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Native;

public static class GpuMath
{
    private const string Lib = "libcuda_lib.so";

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "saxpy")]
    public static extern unsafe void Sanxy(float a, IntPtr x, IntPtr y, IntPtr output, int num);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int iq_fft_c2c_forward(IntPtr inHost, IntPtr outHost, int n);
    
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int iq_fft_c2c_forward2(IntPtr inHost, IntPtr outHost, int n);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int iq_power_spectrum(IntPtr inHost, IntPtr outHost, int n);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int iq_power_db(IntPtr inHost, IntPtr outHost, int n, float floorDb);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int iq_fftshift_inplace(IntPtr ioHost, int n);
}