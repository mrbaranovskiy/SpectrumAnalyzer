using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public sealed class StreamingIQPool : IStreamingDataPool<ComplexF>
{
    private readonly ITransport<ComplexF> _transport;
    private readonly ArrayPool<ComplexF> _pool;
    private ulong _chunkId = 0; //forever  and ever.
    private readonly ConcurrentQueue<ComplexF[]> _queue;

    public StreamingIQPool(ITransport<ComplexF> transport)
    {
        _pool = ArrayPool<ComplexF>.Create(1024*1024*10, 1024*10);
        _transport = transport;
        _transport.DataReceived += TransportOnDataReceived;
        _queue = new ConcurrentQueue<ComplexF[]>();
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

    public bool RequestLatestCopy(Span<ComplexF> buffer)
    {
        if (!_queue.TryDequeue(out var data))
            return false;
        
        if(data.Length != buffer.Length)
            throw new InvalidOperationException("Buffers lenghts do not match");
        data.CopyTo(buffer);
        
        _pool.Return(data, true);

        return false;
    }

    /// <summary>
    /// Peeks but doesnt delete the data from queue.
    /// </summary>
    /// <returns>Returns empty if no data.</returns>
    public ReadOnlySpan<ComplexF> PeekLatestData()
    {
        return !_queue.TryPeek(out var data) 
            ? ReadOnlySpan<ComplexF>.Empty 
            : data;
    }

    public void ReleaseLatestData()
    {
        if (_queue.TryDequeue(out var buffer))
            _pool.Return(buffer, true);
    }

    public void Dispose()
    {
        while (!_queue.IsEmpty)
        {
            _queue.TryDequeue(out var data);
            if (data != null) _pool.Return(data, true);
        }
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    private void OnDataReceived(int size)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs(size, DateTime.UtcNow.ToFileTime()));
    }
}
