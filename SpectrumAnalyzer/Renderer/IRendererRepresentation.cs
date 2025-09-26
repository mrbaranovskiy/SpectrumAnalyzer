using System;
using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Media;
using DynamicData;
using FftSharp.Windows;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

// time/fft/waterfall
public interface IRendererRepresentation<TDrawingProperties, TData> : IDisposable
{
    /// <summary>
    /// Builds the representation
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    ReadOnlySpan<byte> BuildRepresentation();
}

public abstract class RendererRepresentationAbstract<TDrawingProperties, TData>
    : IRendererRepresentation<TDrawingProperties, TData> where TData : struct
{
    private protected readonly IStreamingDataPool<TData> _dataPool;
    private protected ArrayPool<byte> _arrayPool;
    private protected Memory<byte> _bitmapMemoryHandle;
    private protected Memory<TData> _signalMemoryHandle;
    private protected byte[] _screenBuffer;
    private protected TData[] _signalBuffer;
    private TDrawingProperties _drawingProperties;

    protected RendererRepresentationAbstract(IStreamingDataPool<TData> dataPool)
    {
        _dataPool = dataPool;
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

    public abstract ReadOnlySpan<byte> BuildRepresentation();

    protected abstract void HandleDrawingPropertiesUpdated();

    public virtual void Dispose()
    {
        _arrayPool.Return(_screenBuffer);
        ArrayPool<TData>.Shared.Return(_signalBuffer);
    }
}

//todo: it is to complicated. convert to class an add checks.
public record FFTRepresentationProperties(
    int Width,
    int Height,
    double Bandwidth,
    double CenterFrequency,
    double SamplingRate,
    AxisRange XAxisRange,
    AxisRange YAxisRange,
    double XScaleFrequency, // zoom in to frequency.
    double XScale = 1.0, //todo: change it to something [0.1 .. 1.0]
    double YScale = 1.0);


public record WaterfallColorLookup(Color Min,  Color Max)
{
    private readonly Color _min = Min;
    private readonly Color _max = Max;

    public Color Min
    {
        get => _min;
        init => _min = value;
    }

    public Color Max
    {
        get => _max;
        init => _max = value;
    }
}

public record WaterfallRepresentationProperties(
    double Width,
    double Height,
    WaterfallColorLookup ColorLookup,
    double MinFrequency,
    double MaxFrequency,
    double SamplingRage)
{
    private readonly double _width = Width;
    private readonly double _height = Height;
    private readonly WaterfallColorLookup _colorLookup = ColorLookup;
    private readonly double _minFrequency = MinFrequency;
    private readonly double _maxFrequency = MaxFrequency;
    private readonly double _samplingRage = SamplingRage;

    public double Width
    {
        get => _width;
        init => _width = value;
    }

    public double Height
    {
        get => _height;
        init => _height = value;
    }

    public WaterfallColorLookup ColorLookup
    {
        get => _colorLookup;
        init => _colorLookup = value;
    }

    public double MinFrequency
    {
        get => _minFrequency;
        init => _minFrequency = value;
    }

    public double MaxFrequency
    {
        get => _maxFrequency;
        init => _maxFrequency = value;
    }

    public double SamplingRage
    {
        get => _samplingRage;
        init => _samplingRage = value;
    }
}

public class WaterfallRepresentation : RendererRepresentationAbstract<WaterfallRepresentation, Complex>
{
    public WaterfallRepresentation(IStreamingDataPool<Complex> dataPool) : base(dataPool)
    {
    }

    public override ReadOnlySpan<byte> BuildRepresentation()
    {
        throw new NotImplementedException();
    }

    protected override void HandleDrawingPropertiesUpdated()
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
