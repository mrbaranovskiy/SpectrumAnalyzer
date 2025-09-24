using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Tests;

[TestClass]
public class SignalUtilitiesTest
{
    [TestMethod]
    public void TestInterpolation()
    {
        var data = Enumerable.Range(0, 10).Select(s=>(double)s).ToArray().AsSpan();
        var output = new Span<double>(new double[20]);
        SignalDecimation.ResampleData(data, output);
        
        
        Assert.IsTrue(false);
    } 
}
