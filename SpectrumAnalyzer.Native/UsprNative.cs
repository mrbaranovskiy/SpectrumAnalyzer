using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Native;

public static class UsrpNative
{
    const string Lib = "libusrpc.so"; // ensure it can be found in LD_LIBRARY_PATH
    
    // [LibraryImport(Lib)] 
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int usrp_open(string args, out IntPtr handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void usrp_close(IntPtr handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int usrp_get_last_error(IntPtr handle, byte[] buf, int len);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int usrp_configure_rx(
        IntPtr handle,
        double rate,
        double freq,
        double gain,
        double bw,        // pass negative to skip
        string antenna,   // null to skip
        string subdev,    // null to skip
        string refclk,    // null => "internal"
        int integer_n,    // 0/1
        uint channel      // usually 0
    );

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int usrp_prepare_stream(
        IntPtr handle,
        string otw,       // "fc32"
        string cpu,       // "fc32"
        uint channel
    );

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern int usrp_recv_once(
        IntPtr handle,
        float[] outIq,    // interleaved I/Q
        UIntPtr nsamps,   // complex sample count
        int timeoutMs,
        out UIntPtr outReceived
    );

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern double usrp_get_rx_rate(IntPtr handle, uint channel);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    public static extern double usrp_get_rx_freq(IntPtr handle, uint channel);
}