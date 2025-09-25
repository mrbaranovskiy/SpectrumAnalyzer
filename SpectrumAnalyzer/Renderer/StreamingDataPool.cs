using System;
using System.Buffers;
using System.Collections.Concurrent;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public class StreamingDataPool : IStreamingDataPool<float>
{
    private readonly ITransport<float> _transport;
    private readonly ArrayPool<float> _pool;
    private ulong _chunkId = 0; //forever  and ever.
    private ConcurrentQueue<float[]> _queue;

    public StreamingDataPool(ITransport<float> transport)
    {
        _pool = ArrayPool<float>.Create(1024*1024*10, 1024*10);
        _transport = transport;
        _transport.DataReceived += TransportOnDataReceived;
    }

    private void TransportOnDataReceived(object? sender, EventArgs e)
    {
        // delete this bliat`. No sense to read faster in transport than process in
        // Streaming pool. 
        var data = _transport.GetRawData();
        var buffer = _pool.Rent(data.Length);
        _queue.Enqueue(buffer);
        OnDataReceived(data.Length);
    }

    public bool IsAvailable => !_queue.IsEmpty;

    public int RequestLatestDataLength()
    {
        // we can save it before to avoid double enqueuing. 
        return _queue.TryPeek(out var buffer) ? buffer.Length : 0;
    }

    public int RequestedDataLength { get; }

    public bool RequestLatest(Span<float> buffer)
    {
        if (!_queue.TryDequeue(out var data))
            return false;
        
        if(data.Length != buffer.Length)
            throw new InvalidOperationException("Buffers lenghts do not match");
        data.CopyTo(buffer);
        
        _pool.Return(data);

        return false;
    }
    
    public void Dispose()
    {
        while (!_queue.IsEmpty)
        {
            _queue.TryDequeue(out var data);
            _pool.Return(data);
        }
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    protected virtual void OnDataReceived(int size)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs(size, DateTime.UtcNow.ToFileTime()));
    }
}
