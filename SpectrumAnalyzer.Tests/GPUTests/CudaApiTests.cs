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
        var data = new float[1024];
        var signal = new double[data.Length];
        FftSharp.SampleData.AddSin(signal, 32000, 10, 0.1);
        
        for (int i = 0; i < 1024; i+=2)
        {
            data[i] = (float)signal[i];
        }
        using var bx = new UnmanagedFloatBuffer(data.Count());
        using var output = new UnmanagedFloatBuffer(data.Count() * 2);
        using var power = new UnmanagedFloatBuffer(data.Count() * 2);
        var results =  new float[data.Length * 2];
        bx.CopyFrom(data);
        GpuMath.iq_fft_c2c_forward2(bx.Ptr, output.Ptr, data.Length);
        GpuMath.iq_power_db(output.Ptr, power.Ptr, power.Length, -160);
        
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