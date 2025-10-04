using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

// time/fft/waterfall
public interface IRendererRepresentation<TData, TRenderData> : IDisposable where TData : struct
{
    /// <summary>
    /// Builds the representation
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    void BuildRepresentation(ReadOnlySpan<TData> span);
    TRenderData CurrentFrame { get; }
    bool Rendered { get; }
}


//todo: handle DrawingProperties change!!!!
public abstract class RendererRepresentationAbstract<TDrawingProperties, TData, TRenderData> 
    : IRendererRepresentation<TData, TRenderData> 
    where TData : struct
    where TRenderData : struct 
    where TDrawingProperties : IDrawingProperties
{
    protected BitmapGraphics BitmapGraphics;
    protected ArrayPool<byte> BitmapPool;
    protected Memory<byte> BitmapMemoryHandle;
    protected Memory<TData> SignalMemoryHandle;
    protected Memory<float> PowerMemoryHandle;
    protected byte[] BitmapBuffer;
    protected TData[] SignalBuffer;
    protected float[] PowerBuffer;
    TDrawingProperties _drawingProperties;

    protected RendererRepresentationAbstract([DisallowNull] TDrawingProperties drawingProperties)
    {
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

    protected virtual int NumberOfPointsToDraw => 512;

    public void UpdateDrawingProperties(TDrawingProperties properties)
    {
        if(properties.Width <= 0 || properties.Height <= 0)
            return;
        
        //do nothing if buffers size not affected.
        // if(DrawingProperties.Width == properties.Width
        //    && DrawingProperties.Height == properties.Height 
        //    && DrawingProperties.DataBufferLength == properties.DataBufferLength
        //   )
        //     
        //     return;

        if (BitmapBuffer != null) BitmapPool?.Return(BitmapBuffer);
        if (SignalBuffer != null) ArrayPool<TData>.Shared.Return(SignalBuffer);
        if (PowerBuffer != null) ArrayPool<float>.Shared.Return(PowerBuffer);
        
        DrawingProperties = properties;
        
        InitBuffers();
    }

    protected abstract void Draw(Memory<Point> generatePoints, Span<float> magnitudes, Span<float> freqs);

    public virtual void InitBuffers()
    {
        BitmapGraphics = BitmapGraphics.CreateGraphics(DrawingProperties.Width, DrawingProperties.Height, 1.0);
        var windowSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        
        BitmapPool = ArrayPool<byte>.Create(windowSize, 1);
        BitmapBuffer = BitmapPool.Rent(windowSize);
        BitmapMemoryHandle = new Memory<byte>(BitmapBuffer, 0, windowSize);
        SignalBuffer = ArrayPool<TData>.Shared.Rent(DrawingProperties.DataBufferLength);
        PowerBuffer = ArrayPool<float>.Shared.Rent(DrawingProperties.DataBufferLength);
        SignalMemoryHandle = new Memory<TData>(SignalBuffer, 0, DrawingProperties.DataBufferLength);
        PowerMemoryHandle = new Memory<float>(PowerBuffer, 0, DrawingProperties.DataBufferLength);
    }

    public abstract void BuildRepresentation(ReadOnlySpan<TData> data);
    public abstract TRenderData CurrentFrame { get; }
    public bool Rendered { get; protected set; }

    protected abstract void HandleDrawingPropertiesUpdated();

    public virtual void Dispose()
    {
        BitmapPool.Return(BitmapBuffer);
        ArrayPool<TData>.Shared.Return(SignalBuffer);
    }
}