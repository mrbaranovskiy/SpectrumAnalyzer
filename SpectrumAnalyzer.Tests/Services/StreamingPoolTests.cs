using System.Numerics;
using System.Reflection.PortableExecutable;
using Moq;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Tests.Services;


class FakeTransport : ITransport<Complex>
{
    private Task _readingTask;
    private CancellationTokenSource _cancellationTokenSource;
    private double[] _buffer;
    private Complex[] _output;
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public FakeTransport(int len)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _buffer = new double[len];
        _output = new Complex[len];
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public bool IsStreaming => _readingTask.IsCompleted;
    public DateTime LastDataReceived { get; private set; }
    public ReadOnlySpan<Complex> GetRawData()
    {
        return _output;
    }

    public int ReceivingChunkSize { get; set; }
    public Task Start()
    {
        _readingTask = Task.Run(Loop);
        return _readingTask;
    }

    private async Task Loop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            _buffer.AsSpan().Clear();
            double sum = -0.5;
            double dev = 0;
            while (sum < 0.5)
            {
                FftSharp.SampleData.AddSin(_buffer,32000,1000 + dev , 2 - Math.Abs(sum));
                sum += 0.05;
                dev += 50;
            }
            
            for (var i = 0; i < _output.Length; i++) _output[i] = _buffer[i];
            LastDataReceived = DateTime.Now;
            OnDataReceived(new DataReceivedEventArgs(_buffer.Length, 0));
            
            await Task.Delay(10, _cancellationTokenSource.Token);
        }
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public Task Restart()
    {
        return Task.CompletedTask;
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }
}

[TestClass]
public class StreamingPoolTests
{
    [TestMethod]
    public async Task TestStreamingDataPool()
    {
        var moq = new Mock<IDeviceNativeApi<float>>();
        using ITransport<Complex> transport = new FakeTransport(1000);
       
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
        
        for (int i = 0; i < 240; i++)
        {
            render.Render();
            await Task.Delay(20);
        }
        
        var frame = wf.CurrentFrame.ToArray();
        
        Assert.IsTrue(messagesCount > 2);
    }
}
