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
    private protected ArrayPool<TData> _arrayPool;
    private protected Memory<TData> _screenMemoryHandle;
    private protected Memory<TData> _signalMemoryHandle;
    private protected TData[] _screenBuffer;
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

    public abstract void HandleDrawingPropertiesUpdated();

    public virtual void Dispose()
    {
        _arrayPool.Return(_screenBuffer);
        ArrayPool<TData>.Shared.Return(_signalBuffer);
    }
}

public record FFTRepresentationProperties(
    int Width,
    int Height,
    double MinFrequency,
    double MaxFrequency,
    double CenterFrequency,
    double SamplingRate)
{
    private readonly int _width = Width;
    private readonly int _height = Height;
    private readonly double _minFrequency = MinFrequency;
    private readonly double _maxFrequency = MaxFrequency;
    private readonly double _centerFrequency = CenterFrequency;
    private readonly double _samplingRate = SamplingRate;

    public int Width
    {
        get => _width;
        init => _width = value;
    }

    public int Height
    {
        get => _height;
        init => _height = value;
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

    public double CenterFrequency
    {
        get => _centerFrequency;
        init => _centerFrequency = value;
    }

    public double SamplingRate
    {
        get => _samplingRate;
        init => _samplingRate = value;
    }
}

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

public class FFTRepresentation : RendererRepresentationAbstract<FFTRepresentationProperties, Complex>
{
    private readonly Graphics _graphics;

    public FFTRepresentation(IStreamingDataPool<Complex> dataPool) : base(dataPool)
    {
        
        _graphics = Graphics.CreateGraphics(DrawingProperties.Width, DrawingProperties.Height, 1.0);
        
        var windowSize = (int)(DrawingProperties.Height * DrawingProperties.Width * 4);
        
        _arrayPool = ArrayPool<Complex>.Create(windowSize, 1);
        _screenBuffer = _arrayPool.Rent(windowSize);
        _screenMemoryHandle = new Memory<Complex>(_screenBuffer, 0, windowSize);
        _signalBuffer = ArrayPool<Complex>.Shared.Rent(dataPool.RequestedDataLength);
        _signalMemoryHandle = new Memory<Complex>(_signalBuffer, 0, windowSize);
    }

    public override ReadOnlySpan<byte> BuildRepresentation()
    {
        _dataPool.RequestLatest(_signalMemoryHandle.Span);
        // FftSharp.Windows.Rectangular rw = new Rectangular();
        FftSharp.FFT.Forward(_signalMemoryHandle.Span);
        // todo: GC intensive code. Need to reimplement this.
        var power = FftSharp.FFT.Power(_screenBuffer);
        var freq = FftSharp.FFT.FrequencyScale(power.Length, DrawingProperties.SamplingRate);

        // cut only needed frequencies, because we can zoon in/out on the screen.

        var min = (int)DrawingProperties.MinFrequency;
        var max = (int)DrawingProperties.MaxFrequency;
        var imin = 0;
        var imax = 0;
        
        //??? check it...
        for (int i = 0; i < freq.Length; i++)
        {
            if (freq[i] < min) 
                continue;
            
            imin = Math.Max(i - 1, 0);
            break;
        }

        for (int i = freq.Length - 1; i >= 0; i--)
        {
           if(freq[i] > max)
               continue;
           imax = Math.Min(i + 1, freq.Length - 1);
        }
        
        // this cut spectrum
        var powerSpan = new Span<double>(power, imin, imax);
                
        

        // not fit the data to the screen.

    }

    public override void HandleDrawingPropertiesUpdated()
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
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

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
