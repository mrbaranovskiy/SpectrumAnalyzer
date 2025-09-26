using System.Numerics;
using Moq;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Tests.Services;

[TestClass]
public class StreamingPoolTests
{
    [TestMethod]
    public async Task TestStreamingDataPool()
    {
        var moq = new Mock<IDeviceNativeApi<float>>();
        ITransport<Complex> transport = new UHDTransport(moq.Object);
        var sdp = new StreamingDataPool(transport);
        int messagesCount = 0;
        sdp.DataReceived += (sender, args) =>
        {
            messagesCount++;
        };
        
        await Task.Delay(500);
        
        Assert.IsTrue(messagesCount > 2);
    }
}
