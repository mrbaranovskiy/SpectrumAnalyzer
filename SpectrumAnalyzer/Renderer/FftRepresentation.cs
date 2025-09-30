using System;
using System.Buffers;
using System.Numerics;
using Avalonia;
using SpectrumAnalyzer.Utilities;
using Avalonia.Media;


namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation<TDrawingProperties> 
    : RendererRepresentationAbstract<TDrawingProperties, Complex> 
    where TDrawingProperties : IDrawingProperties
{
    public FftRepresentation(TDrawingProperties properties) 
        : base(properties)
    {
        UpdateDrawingProperties(properties);
    }

    public override void Dispose()
    {
        BitmapPool.Return(BitmapBuffer);
        ArrayPool<Complex>.Shared.Return(SignalBuffer);
    }

    public override void BuildRepresentation(ReadOnlySpan<Complex> data)
    {
        if(Rendered)
            return;
        
        if (data.Length != SignalMemoryHandle.Length)
            throw new NotImplementedException("Implement resize");
        
        //todo: probably this is redundant copy
        data.CopyTo(SignalMemoryHandle.Span);

        FftSharp.FFT.Forward(SignalMemoryHandle.Span);
        // todo: GC intensive code. Need to Implement FFT over Spans.
        var power = FftSharp.FFT.Power(SignalBuffer);
        var freq = FftSharp.FFT.FrequencyScale(power.Length, DrawingProperties.SamplingRate);

        var wndSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        var screenPoints = ArrayPool<Point>.Shared.Rent(wndSize);
     
        
        //todo: need to decide how many points to draw on the screen. 
        //there is no much sense to draw them all.
        // temporary I took 3 screen width. 
        // Mayne some Shannon theorem to avoid signal lost.
        int numberOfDrawedPoints = 512;
        var screenPointsMem = new Memory<Point>(screenPoints, 0, numberOfDrawedPoints);

        var resampledPower = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resPowerMem = new Memory<double>(resampledPower, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(power, resPowerMem.Span);
        
        var resampledFreq = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resFreqMem = new Memory<double>(resampledFreq, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(freq, resFreqMem.Span);
        
        var ys = resPowerMem.Span;
        var xs = resFreqMem.Span;
        
        Draw(screenPointsMem, ys, xs);
        Rendered = true;
        
        
        ArrayPool<Point>.Shared.Return(screenPoints, true); 
        ArrayPool<double>.Shared.Return(resampledPower, true);
        ArrayPool<double>.Shared.Return(resampledFreq, true);
    }

    protected override void Draw(Memory<Point> generatePoints, Span<double> ys, Span<double> xs)
    {
        BitmapMemoryHandle.Span.Clear();
        GeneratePoints(generatePoints.Span, ys, xs );

        // _bitmapGraphics.DrawLines(_bitmapMemoryHandle.Span, screenPointsMem.Span, Colors.White);
        BitmapGraphics.DrawLines(BitmapMemoryHandle, generatePoints, Colors.Green);
    }

    public override ReadOnlySpan<byte> CurrentFrame
    {
        get
        {
            Rendered = false;
            return BitmapMemoryHandle.Span;
        }
    }

    protected void GeneratePoints(Span<Point> output,
        ReadOnlySpan<double> ys,
        ReadOnlySpan<double> xs)
    {
        for (int i = 0; i < ys.Length; i++)
        {
            var scaledPt = RangesMapper.Map2Point(new Point(xs[i], ys[i]), 
                DrawingProperties.Height,
                DrawingProperties.Width,
                DrawingProperties.YAxisRange.Min,
                DrawingProperties.YAxisRange.Max,
                0,
                // ReSharper disable once PossibleLossOfFraction
                DrawingProperties.SamplingRate / 2
            );
            
            output[i] = new Point(scaledPt.Item1, scaledPt.Item2);
        }
    }

    protected override void HandleDrawingPropertiesUpdated()
    {
        
    }
}
