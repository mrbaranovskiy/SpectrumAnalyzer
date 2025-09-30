using System;
using System.Buffers;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Services;

public class UsrpTransport : ITransport<Complex>
{
    private readonly IDeviceNativeApi<float> _api;
    private readonly UsrpConnectionProperties _connectionProps;
    private Task _readTask;
    private readonly CancellationTokenSource _cts = new();
    private Memory<Complex> _memoryComplex;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    private static readonly object Lock = new();
    private Complex[] _bufferComplex;
    private float[] _bufferFloat;
    private int _receivingChunkSize = 1 << 12;
    private readonly Memory<float> _memoryFloat;

    public UsrpTransport(
        IDeviceNativeApi<float> api,
        UsrpConnectionProperties connectionProps)
    {
        _readTask = Task.CompletedTask;
        _api = api;
        _connectionProps = connectionProps;
        _bufferComplex = new Complex[ReceivingChunkSize];
        _memoryComplex = new Memory<Complex>(_bufferComplex);
        _bufferFloat = new float[2*ReceivingChunkSize];
        _memoryFloat = new Memory<float>(_bufferFloat);
    }

    public bool IsStreaming => _readTask.Status == TaskStatus.Running;

    public DateTime LastDataReceived { get; private set; }

    public ReadOnlySpan<Complex> GetRawData()
    {
        lock (Lock)
            return _memoryComplex.Span;
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
        _api.Open();
        return Restart();
    }

    public Task Stop()
    {
        return _cts.CancelAsync();
    }

    public Task Restart()
    {
        var fr = _connectionProps.CenterFrequencyHz;
        var sr = _connectionProps.SampleRateHz;
        var gain = _connectionProps.GainDb;
        var bw = _connectionProps.BandwidthHz;
        
        _api.ConfigureRx(fr,sr,  gain, bw, antenna: _connectionProps.Antenna);
        _api.PrepareStream(0);
        _readTask = Task.Run(ReadingLoop);
        return _readTask;
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }

    private void ResetPools()
    {
        lock (Lock)
        {
            _bufferComplex = new Complex[_receivingChunkSize];
            _bufferFloat = new float[2 * _receivingChunkSize];
            _memoryComplex = new Memory<Complex>(_bufferComplex);
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
                _memoryComplex.Span.Clear();
                _memoryFloat.Span.Clear();
                
                _api.Receive(_bufferFloat, ReceivingChunkSize, out var bytesRead);
                
                //Thread.Sleep(1000);

                if (bytesRead > 0)
                {
                    if (_bufferFloat.All(s=> s==0))
                    {
                        Console.WriteLine("Something went wrong");
                    }

                    if (bytesRead < this.ReceivingChunkSize)
                    {
                        Console.WriteLine($"Small chunk received {bytesRead}");
                    }
                }
                    
                //var bytesRead = _bufferFloat.Length;
                // var temp = new double[_bufferFloat.Length];
                // FftSharp.SampleData.AddSin(temp, (int) _connectionProps.SampleRateHz, accumulator+=2000, 0.03);

                for (int i = 0; i < bytesRead; i+=2)
                {
                    _bufferComplex[i / 2] = new Complex(_bufferFloat[i],_bufferFloat[i + 1]);
                }
                
                LastDataReceived = DateTime.UtcNow;
                OnDataReceived(new DataReceivedEventArgs(bytesRead, DateTime.UtcNow.ToFileTimeUtc()));
            }
        }
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _api.Dispose();
    }
}
