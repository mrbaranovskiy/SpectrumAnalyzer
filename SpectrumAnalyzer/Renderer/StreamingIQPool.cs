using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public sealed class StreamingIQPool : IStreamingDataPool<ComplexF>
{
    private readonly ITransport<ComplexF> _transport;
    private readonly ArrayPool<ComplexF> _pool;
    private readonly ConcurrentQueue<Memory<ComplexF>> _memQueue;
    private readonly Dictionary<Memory<ComplexF>, ComplexF[]> _memLookup;

    public StreamingIQPool(ITransport<ComplexF> transport)
    {
        _pool = ArrayPool<ComplexF>.Create(1024*1024*10, 10);
        _transport = transport;
        _transport.DataReceived += TransportOnDataReceived;
        _memQueue = new ConcurrentQueue<Memory<ComplexF>>();
        _memLookup = new Dictionary<Memory<ComplexF>, ComplexF[]>();
        MaxQueueSize = 10;
    }

    private void TransportOnDataReceived(object? sender, EventArgs e)
    {
        if(_memQueue.Count >= MaxQueueSize) return;
        // delete this bliat`. No sense to read faster in transport than process in
        // Streaming pool. 
        var data = _transport.GetRawData();
        var buffer = _pool.Rent(data.Length);
        var memory = new Memory<ComplexF>(buffer, 0, _transport.ReceivingChunkSize);
        data.CopyTo(memory.Span);
        _memLookup[memory] = buffer;
        _memQueue.Enqueue(memory);
        OnDataReceived(data.Length);
    }

    public bool IsAvailable => !_memQueue.IsEmpty;

    public int RequestLatestDataLength()
    {
        // we can save it before to avoid double enqueuing. 
        return _memQueue.TryPeek(out var buffer) ? buffer.Length : 0;
    }

    public int MaxQueueSize { get; set; }

    public bool RequestLatestCopy(Span<ComplexF> buffer)
    {
        if (!_memQueue.TryDequeue(out var mem))
            return false;
        
        if(mem.Length != buffer.Length)
            throw new InvalidOperationException("Buffers lenghts do not match");
        mem.Span.CopyTo(buffer);

        if (!_memLookup.TryGetValue(mem, out var value))
            return false;
        
        _pool.Return(value, true);
        _memLookup.Remove(mem);

        return false;
    }

    /// <summary>
    /// Peeks but doesnt delete the data from queue.
    /// </summary>
    /// <returns>Returns empty if no data.</returns>
    public ReadOnlySpan<ComplexF> PeekLatestData()
    {
        return !_memQueue.TryPeek(out var data) 
            ? ReadOnlySpan<ComplexF>.Empty 
            : data.Span;
    }

    public void ReleaseLatestData()
    {
        if (_memQueue.TryDequeue(out var mem))
        {
            if (_memLookup.TryGetValue(mem, out var value))
            {
                _pool.Return(_memLookup[mem], true);
                _memLookup.Remove(value);
            }
        }
    }

    public void Dispose()
    {
        while (!_memQueue.IsEmpty)
        {
            if (!_memQueue.TryDequeue(out var mem))
                continue;
            
            _pool.Return(_memLookup[mem], true);
            _memLookup.Remove(mem);

        }
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    private void OnDataReceived(int size)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs(size, DateTime.UtcNow.ToFileTime()));
    }
}
