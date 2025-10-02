using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
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
        var len = 1 << 20;
        var data = new float[len];
        var temp = new double[data.Length];

        FftSharp.SampleData.AddSin(temp, 32000, 1000, 0.001);
        
        // var start = Stopwatch.StartNew();
        // FftSharp.FFT.Forward(temp);
        // start.Stop();
        
        for (int i = 0; i < len; i+=2) data[i] = (float)temp[i];
        
        using var signal_buffer = new UnmanagedFloatBuffer(data.Count());
        
        
        using var output = new UnmanagedFloatBuffer(data.Count());
        using var power = new UnmanagedFloatBuffer(data.Count());
        using var freqs = new UnmanagedFloatBuffer(power.Length);
        
        var results =  new float[data.Length ];
        var freqsResults =  new float[power.Length ];
        signal_buffer.CopyFrom(data);
        
        var err = GpuMath.iq_fft_c2c_forward2(signal_buffer.Ptr, output.Ptr, data.Length);

        //var test = FftSharp.FFT.Forward(signal);
        GpuMath.iq_power_db(output.Ptr, power.Ptr, power.Length, -160);
        power.CopyTo(results);

        GpuMath.k_scale_r(freqs.Ptr, freqsResults.Length, 32000);
        freqs.CopyTo(freqsResults);
        
        
        power.CopyTo(results);
        
        Assert.IsFalse(results.All(s=>s==0.0));
    }
}

[StructLayout(LayoutKind.Sequential)]
struct MyComplex
{
    public float Real;
    public float Imaginary;
}