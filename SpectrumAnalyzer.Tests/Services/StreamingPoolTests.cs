using Moq;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Tests.Services;

[TestClass]
public class StreamingPoolTests
{
    [TestMethod]
    public void TestStreamingDataPool()
    {
        var moq = new Mock<IDeviceNativeApi<float>>();
        ITransport<float> transport = new UHDTransport(moq.Object);
        var sdp = new StreamingDataPool(transport);
    }
}
