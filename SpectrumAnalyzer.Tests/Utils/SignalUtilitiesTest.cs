using System.Numerics;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Tests;

[TestClass]
public class SignalUtilitiesTest
{
    [TestMethod]
    public void TestInterpolation()
    {
        var data = Enumerable.Range(0, 10).Select(s => (double)s).ToArray().AsSpan();
        var output = new Span<double>(new double[20]);
        SignalDecimation.ResampleData(data, output);


        Assert.IsTrue(false);
    }

    [TestMethod]
    public void Test()
    {
        var data = new double[1 << 12];
        FftSharp.SampleData.AddSin(data, 1000_000, 500e3, 2);
        FftSharp.SampleData.AddSin(data, 1000_000, 100e3, 3);

        var spectrum = FftSharp.FFT.Forward(data);
        var psd = FftSharp.FFT.Power(spectrum);
        var freq = FftSharp.FFT.FrequencyScale(psd.Length, 1000_000);
        
        var idx = 0;
        for (var i = 0; i < psd.Length; i++)
        {
            if (psd[idx] < psd[i])
                idx = i;
        }
        
        var result = freq[idx];
        Assert.IsTrue(false);
    }

    [TestMethod]
    public void TestSignalGenerator()
    {
        var spectrum = new Complex[1 << 15];
        const double sample_rate = 32e3;
        SignalGenerator.GenerateRandomIq(spectrum, centerf: 1000, sr: sample_rate,
            (5e3, 1), 
            (10e3, 2)
        );


        FftSharp.FFT.Forward(spectrum.AsSpan());
        var psd = FftSharp.FFT.Power(spectrum);
        var freq = FftSharp.FFT.FrequencyScale(psd.Length, sample_rate);

        var idx = 0;
        for (var i = 0; i < psd.Length; i++)
        {
            if (psd[idx] < psd[i])
                idx = i;
        }

        var result = freq[idx];
        
        Assert.IsTrue(false);
        
    }
}
