using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Tests;

[TestClass]
public class UhdTest
{
    static string GetErr(IntPtr h)
    {
        var buf = new byte[1024];
        int n = UsrpNative.usrp_get_last_error(h, buf, buf.Length);
        return n > 0 ? System.Text.Encoding.UTF8.GetString(buf, 0, n) : "";
    }
    
    [TestMethod]
    public void ConnectToDeviceTest()
    {
        IntPtr h;
        int rc = UsrpNative.usrp_open("", out h);
        if (rc != 0) throw new Exception("open failed");

        try {
            rc = UsrpNative.usrp_configure_rx(
                h,
                rate: 2e6,            // 2 MS/s
                freq: 100e6,          // 100 MHz
                gain: 30.0,           // 20 dB
                bw: -1,               // skip BW
                antenna: "TX/RX",       // or null
                subdev: null!,
                refclk: "internal",
                integer_n: 0,
                channel: 0);
            if (rc != 0) throw new Exception("configure: " + GetErr(h));

            rc = UsrpNative.usrp_prepare_stream(h, "fc32", "fc32", 0);
            if (rc != 0) throw new Exception("prepare_stream: " + GetErr(h));

            const int N = 4096;
            var iq = new float[2 * N];
            UIntPtr got;
            rc = UsrpNative.usrp_recv_once(h, iq, (UIntPtr)N, 500, out got);
            if (rc != 0) throw new Exception("recv_once: " + GetErr(h));

            int nGot = (int)got;
            Console.WriteLine($"Received {nGot} complex samples");
            // iq[0]=I0, iq[1]=Q0, iq[2]=I1, iq[3]=Q1, ...
        }
        finally {
            UsrpNative.usrp_close(h);
        }
    }
}