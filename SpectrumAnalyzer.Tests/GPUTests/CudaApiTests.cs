using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Tests.GPUTests;

[TestClass]
public class CudaApiTests
{
    [TestMethod]
    public void TestSaxpy()
    {
        var n = 10_000_000;
        using var bx = new UnmanagedFloatBuffer(n);
        using var by = new UnmanagedFloatBuffer(n);
        using var bout = new UnmanagedFloatBuffer(n);

        float[] x = new float[n];
        float[] y = new float[n];
        Array.Fill(x, 1);
        Array.Fill(y, 2);
        
        float[] output = new float[n];
        
        bx.CopyFrom(x);
        by.CopyFrom(y);
        GpuMath.Sanxy(2.0f, bx.Ptr, by.Ptr, bout.Ptr, n);
        bout.CopyTo(output);
        
        Assert.IsFalse(output.All(s=>s==0.0));
    }
    
    [TestMethod]
    public void TestFFt()
    {
        var len = 1 << 10;
        var data = new float[len];
        var temp = new double[data.Length];

        FftSharp.SampleData.AddSin(temp, 32000, 500, 1);
        
        for (int i = 1; i < len; i+=1)
            data[i] = (float)temp[i];
        
        using var signal_raw_c = new UnmanagedFloatBuffer(data.Length);
        
        var test = new float[len];
        signal_raw_c.CopyFrom(data);
        signal_raw_c.CopyTo(test);
        
        
        var spectrum_raw = new UnmanagedFloatBuffer(signal_raw_c.Length / 2);
        var power_raw = new UnmanagedFloatBuffer(signal_raw_c.Length / 2);
        var freqs_raw = new UnmanagedFloatBuffer(signal_raw_c.Length / 2);
        
        GpuMath.iq_fft_c2r_forward2(signal_raw_c.Ptr, spectrum_raw.Ptr, data.Length / 2);
        GpuMath.iq_power_db_real(spectrum_raw.Ptr, power_raw.Ptr, spectrum_raw.Length, -160);
        GpuMath.k_scale_r(freqs_raw.Ptr, power_raw.Length, 32000);
        
        var spec_out =  new float[spectrum_raw.Length];
        var power_out =  new float[spectrum_raw.Length];
        var freqs_out =  new float[power_raw.Length ];

        spectrum_raw.CopyTo(spec_out);
        power_raw.CopyTo(power_out);
        freqs_raw.CopyTo(freqs_out);
        
        Assert.IsFalse(power_out.All(s=>s==0.0));
    }
}

[StructLayout(LayoutKind.Sequential)]
struct MyComplex
{
    public float Real;
    public float Imaginary;
}