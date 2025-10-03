using System;
using System.Threading;
using System.Threading.Tasks;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.Services;

public class FakeTransport(UsrpConnectionProperties properties, int len) : ITransport<ComplexF>
{
    private Task _readingTask;
    private CancellationTokenSource _cancellationTokenSource = new();
    private double[] _buffer = new double[len];
    private ComplexF[] _output = new ComplexF[len];
    private bool _run = false;
    
    public void Dispose()
    {
        _readingTask.Dispose();
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public bool IsStreaming => _run;
    public DateTime LastDataReceived { get; private set; }
    public ReadOnlySpan<ComplexF> GetRawData() => _output;

    public int ReceivingChunkSize { get; set; } = len;
    public async Task Start()
    {
        _run = true;
        await Loop();
    }

    private async Task Loop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            _buffer.AsSpan().Clear();

            var sr = (int)properties.SampleRateHz;
            var target = sr / 4;

            // this is stupid way... would be nice to have smt with more harmonics...
            for (var i = 0; i < 10; i++) 
                FftSharp.SampleData.AddSin(_buffer,(int)properties.SampleRateHz, target + i* Random.Shared.NextInt64(1,5) ,1);

            for (var i = 0; i < 30; i++)
            {
                FftSharp.SampleData.AddSin(_buffer,(int)properties.SampleRateHz, 
                    Random.Shared.NextInt64(1000, (long)(properties.SampleRateHz / 2)) ,0.0001);
            }
            
            for (var i = 0; i < _output.Length; i++)
                _output[i] = _buffer[i];
            
            LastDataReceived = DateTime.Now;
            OnDataReceived(new DataReceivedEventArgs(_buffer.Length, 0));
            
            await Task.Delay(25, _cancellationTokenSource.Token);
        }
    }

    public Task Stop()
    {
        _run = false;
        return _cancellationTokenSource.CancelAsync();;
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