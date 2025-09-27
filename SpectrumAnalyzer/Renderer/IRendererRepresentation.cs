using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace SpectrumAnalyzer.Renderer;

// time/fft/waterfall
public interface IRendererRepresentation<TData> : IDisposable where TData : struct
{
    /// <summary>
    /// Builds the representation
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    void BuildRepresentation(ReadOnlySpan<TData> span);
    ReadOnlySpan<byte> CurrentFrame { get; }
}


//todo: handle DrawingProperties change!!!!
public abstract class RendererRepresentationAbstract<TDrawingProperties, TData>
    : IRendererRepresentation<TData> where TData : struct
{
    private protected readonly IStreamingDataPool<TData> _dataPool;
    private protected ArrayPool<byte> _bitmapPool;
    private protected Memory<byte> _bitmapMemoryHandle;
    private protected Memory<TData> _signalMemoryHandle;
    private protected byte[] _bitmapBuffer;
    private protected TData[] _signalBuffer;
    private TDrawingProperties _drawingProperties;
    protected readonly int _singleBufferLength;

    protected RendererRepresentationAbstract([DisallowNull] TDrawingProperties drawingProperties, int singleBufferLength)
    {
        _singleBufferLength = singleBufferLength;
        _drawingProperties = drawingProperties ?? throw new ArgumentNullException(nameof(drawingProperties));
    }

    protected TDrawingProperties DrawingProperties
    {
        get => _drawingProperties;
        set
        {
            if (value == null) return;
            
            _drawingProperties = value;
            HandleDrawingPropertiesUpdated();
        }
    }


    public abstract void UpdateDrawingProperties(TDrawingProperties properties);
    public abstract void BuildRepresentation(ReadOnlySpan<TData> data);
    public abstract ReadOnlySpan<byte> CurrentFrame { get; }

    protected abstract void HandleDrawingPropertiesUpdated();

    public virtual void Dispose()
    {
        _bitmapPool.Return(_bitmapBuffer);
        ArrayPool<TData>.Shared.Return(_signalBuffer);
    }
}

//todo: it is to complicated. convert to class an add checks.
