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
                rate: 2e6,           
                freq: 100e6,         
                gain: 30.0,          
                bw: -1,              
                antenna: "TX/RX",    
                subdev: null!,
                refclk: "internal",
                integer_n: 0,
                channel: 0);
            if (rc != 0) throw new Exception("configure: " + GetErr(h));

            rc = UsrpNative.usrp_prepare_stream(h, "fc32", "fc32", 0);
            if (rc != 0) throw new Exception("prepare_stream: " + GetErr(h));

            const int N = 4096;
            var iq = new float[2 * N];
            UIntPtr output;
            rc = UsrpNative.usrp_recv_once(h, iq, N, 500, out output);
            if (rc != 0) throw new Exception("recv_once: " + GetErr(h));

            int nGot = (int)output;
            Console.WriteLine($"Received {nGot} complex samples");
        }
        finally {
            UsrpNative.usrp_close(h);
        }
    }
}