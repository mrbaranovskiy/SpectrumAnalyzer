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
    public void TestSignalGenerator()
    {
        var spectrum = new Complex[1 << 15];
        const double sample_rate = 32e3;
        SignalGenerator.GenerateRandomIQ(spectrum, center_fr: 1000, sampling_rate: sample_rate,
            (5e3, 1), 
            (10e3, 2)
        );


        FftSharp.FFT.Forward(spectrum.AsSpan());
        double[] psd = FftSharp.FFT.Power(spectrum);
        double[] freq = FftSharp.FFT.FrequencyScale(psd.Length, sample_rate);

        int idx = 0;
        for (int i = 0; i < psd.Length; i++)
        {
            if (psd[idx] < psd[i])
                idx = i;
        }

        var result = freq[idx];
        
        Assert.IsTrue(false);
        
    }
}
