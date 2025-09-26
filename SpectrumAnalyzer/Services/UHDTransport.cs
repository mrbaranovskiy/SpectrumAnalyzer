using System;
using System.Buffers;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Services;

public class UHDTransport : ITransport<Complex>
{
    private readonly IDeviceNativeApi<float> _api;
    private readonly Task _readTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly ArrayPool<Complex> _pool;
    private readonly Memory<Complex> _memory;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    private static readonly object _lock = new();
    private double _time; //temporary used for signal generation,
    private readonly Complex[] _buffer;


    public UHDTransport(IDeviceNativeApi<float> api)
    {
        _api = api;
        _pool = ArrayPool<Complex>.Create(1 << 16, 1 << 12);
        _buffer = _pool.Rent(1 << 12);
        _memory = new Memory<Complex>(_buffer, 0, 1 << 12);
        // todo: not the best idea.... do some start function.. 
        _readTask = Task.Run(ReadingLoop);
    }

    public bool IsStreaming => _readTask.Status == TaskStatus.Running;

    public DateTime LastDataReceived { get; }

    public ReadOnlySpan<Complex> GetRawData()
    {
        lock (_lock)
        {
            return _memory.Span;
        }
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }
    
    private void ReadingLoop()
    {
        //no fansy asynk await
        while (!_cts.Token.IsCancellationRequested)
        {
            lock (_lock)
            {
                // this is bad. No good control for the data.
                // data can be lost. 
                /// todo: some prebuffereing.
                SignalGenerator.GenerateRandomIQ(_memory.Span, center_fr: 1000, sampling_rate: 32e3,
                    (5e3, 1), 
                    (10e3, 2)
                );
                
                OnDataReceived(new DataReceivedEventArgs(1024, DateTime.UtcNow.ToFileTimeUtc()));
            }
            
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _pool.Return(_buffer);
        //todo:
        //dispose native api.
    }
}
