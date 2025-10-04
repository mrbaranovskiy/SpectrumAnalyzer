using System.Numerics;
using Moq;
using SpectrumAnalyzer.Models;
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
        var connectionProps = new UsrpConnectionProperties
        {
            Antenna = "RX2",
            BandwidthHz = 32000,
            CenterFrequencyHz = 10000,
            GainDb = 30,
            SampleRateHz = 10000
        };
        using ITransport<ComplexF> transport = new FakeTransport(connectionProps, 1000);
       
        var sdp = new StreamingIQPool(transport);
        var messagesCount = 0;
        sdp.DataReceived += (sender, args) =>
        {
            messagesCount++;
        };

        transport.Start();
        
        await Task.Delay(500);
        
        var props = new FFTDrawingProperties(
            ITransport<Complex>.DefaultChunkSize,
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
        props = props with { DataBufferLength = transport.ReceivingChunkSize };
        
        //var fft = new FftRepresentation<FFTDrawingProperties>(props);
        var wf = new WaterfallRepresentation(
            new WaterfallDrawingProperties(1024, 320, 240, 64000, 100, 32000, new AxisRange(0, 16000), new AxisRange(-30.0, 60.0))
        );
        
        render.AddRepresentation(wf);
        
        for (var i = 0; i < 240; i++)
        {
            render.Render();
            await Task.Delay(20);
        }
        
        var frame = wf.CurrentFrame.ToArray();
        
        Assert.IsTrue(messagesCount > 2);
    }
}
