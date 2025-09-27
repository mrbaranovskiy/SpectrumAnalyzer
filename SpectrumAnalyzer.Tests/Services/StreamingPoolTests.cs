using System.Numerics;
using Moq;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Tests.Services;

[TestClass]
public class StreamingPoolTests
{
    [TestMethod]
    public async Task TestStreamingDataPool()
    {
        var moq = new Mock<IDeviceNativeApi<float>>();
        using ITransport<Complex> transport = new UHDTransport(moq.Object);
        var sdp = new StreamingIQPool(transport);
        var messagesCount = 0;
        sdp.DataReceived += (sender, args) =>
        {
            messagesCount++;
        };
        
        await Task.Delay(500);

        var props = new FFTRepresentationProperties(
            Width: 1000,
            Height: 300,
            Bandwidth: 10000,
            CenterFrequency: 10000,
            SamplingRate: 32000,
            XAxisRange: new AxisRange(1, 16000),
            YAxisRange: new AxisRange(-400, 60),
            XScaleFrequency: 10e3
        );
        var render = new ComplexDataRenderer(sdp);
        var representation = new FftRepresentation(props, transport.ReceivingChunkSize);
        render.AddRepresentation(representation);
        render.Render();

        var frame = representation.CurrentFrame.ToArray();

        Assert.IsTrue(messagesCount > 2);
    }
}
