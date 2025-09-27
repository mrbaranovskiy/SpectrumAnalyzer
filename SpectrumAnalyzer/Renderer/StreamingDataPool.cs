using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public sealed class StreamingDataPool : IStreamingDataPool<Complex>
{
    private readonly ITransport<Complex> _transport;
    private readonly ArrayPool<Complex> _pool;
    private ulong _chunkId = 0; //forever  and ever.
    private readonly ConcurrentQueue<Complex[]> _queue;

    public StreamingDataPool(ITransport<Complex> transport)
    {
        _pool = ArrayPool<Complex>.Create(1024*1024*10, 1024*10);
        _transport = transport;
        _transport.DataReceived += TransportOnDataReceived;
        _queue = new ConcurrentQueue<Complex[]>();
        MaxQueueSize = 10;
    }

    private void TransportOnDataReceived(object? sender, EventArgs e)
    {
        if(_queue.Count >= MaxQueueSize) return;
        // delete this bliat`. No sense to read faster in transport than process in
        // Streaming pool. 
        var data = _transport.GetRawData();
        var buffer = _pool.Rent(data.Length);
        data.CopyTo(buffer);
        _queue.Enqueue(buffer);
        OnDataReceived(data.Length);
    }

    public bool IsAvailable => !_queue.IsEmpty;

    public int RequestLatestDataLength()
    {
        // we can save it before to avoid double enqueuing. 
        return _queue.TryPeek(out var buffer) ? buffer.Length : 0;
    }

    public int MaxQueueSize { get; set; }

    public bool RequestLatestCopy(Span<Complex> buffer)
    {
        if (!_queue.TryDequeue(out var data))
            return false;
        
        if(data.Length != buffer.Length)
            throw new InvalidOperationException("Buffers lenghts do not match");
        data.CopyTo(buffer);
        
        _pool.Return(data);

        return false;
    }

    /// <summary>
    /// Peeks but doesnt delete the data from queue.
    /// </summary>
    /// <returns>Returns empty if no data.</returns>
    public ReadOnlySpan<Complex> PeekLatestData()
    {
        return !_queue.TryPeek(out var data) 
            ? ReadOnlySpan<Complex>.Empty 
            : data;
    }

    public void ReleaseLatestData()
    {
        if (_queue.TryDequeue(out var buffer))
            _pool.Return(buffer);
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

    private void OnDataReceived(int size)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs(size, DateTime.UtcNow.ToFileTime()));
    }
}
