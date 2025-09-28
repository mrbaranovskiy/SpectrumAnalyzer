using System;
using System.Buffers;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Services;

public class UsrpTransport : ITransport<Complex>
{
    private readonly IDeviceNativeApi<float> _api;
    private Task _readTask;
    private readonly CancellationTokenSource _cts = new();
    private ArrayPool<Complex> _pool;
    private Memory<Complex> _memory;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    private static readonly object Lock = new();
    private double _time; //temporary used for signal generation,
    private Complex[] _buffer;
    private int _receivingChunkSize = 1 << 12;

    public UsrpTransport(IDeviceNativeApi<float> api)
    {
        _readTask = Task.CompletedTask;
        _api = api;
        //todo: resolve this mess with receiving size
        _pool = ArrayPool<Complex>.Shared;
        _buffer = _pool.Rent(ReceivingChunkSize);
        _memory = new Memory<Complex>(_buffer, 0, ReceivingChunkSize);
    }

    public bool IsStreaming => _readTask.Status == TaskStatus.Running;

    public DateTime LastDataReceived { get; }

    public ReadOnlySpan<Complex> GetRawData()
    {
        lock (Lock)
            return _memory.Span;
    }

    public int ReceivingChunkSize
    {
        get => _receivingChunkSize;
        set
        {
            _receivingChunkSize = value;
            ResetPools();
        }
    }

    public Task Start()
    {
        _readTask = Task.Run(ReadingLoop);
        return _readTask;
    }

    public Task Stop()
    {
        return _cts.CancelAsync();
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }

    private void ResetPools()
    {
        lock (Lock)
        {
            _pool.Return(_buffer, true);
            _pool = ArrayPool<Complex>.Shared;
            _buffer = _pool.Rent(_receivingChunkSize);
            _memory = new Memory<Complex>(_buffer, 0, _receivingChunkSize);
        }
    }
    
    private void ReadingLoop()
    {
        //no fansy asynk await
        while (!_cts.Token.IsCancellationRequested)
        {
            lock (Lock)
            {
                // this is bad. No good control for the data.
                // data can be lost. 
                /// todo: some prebuffereing.
                /// 

                _memory.Span.Clear();
                var temp = new double[_memory.Span.Length];

                for (int i = 0; i < 10; i++)
                {
                    FftSharp.SampleData.AddSin(temp, 32000, 10.34e3 + i * 13, 4);
                }

                for (int i = 0; i < _memory.Span.Length; i++) _memory.Span[i] += temp[i];
                
                OnDataReceived(new DataReceivedEventArgs(_memory.Span.Length, DateTime.UtcNow.ToFileTimeUtc()));
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
