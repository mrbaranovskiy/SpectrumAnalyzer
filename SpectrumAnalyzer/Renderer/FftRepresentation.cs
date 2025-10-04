using System;
using System.Buffers;
using System.Numerics;
using Avalonia;
using SpectrumAnalyzer.Utilities;
using Avalonia.Media;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation<TDrawingProperties> 
    : RendererRepresentationAbstract<TDrawingProperties, ComplexF> 
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
        ArrayPool<ComplexF>.Shared.Return(SignalBuffer);
    }

    public override void BuildRepresentation(ReadOnlySpan<ComplexF> data)
    {
        if(Rendered)
            return;
        
        if (data.Length != SignalMemoryHandle.Length)
            throw new NotImplementedException("Implement resize");
        
        //todo: probably this is redundant copy
        data.CopyTo(SignalMemoryHandle.Span);
        
        AcceleratedMath.FftForward(SignalMemoryHandle, SignalMemoryHandle);
        AcceleratedMath.FftPower(SignalMemoryHandle, PowerMemoryHandle);
        // todo: GC intensive code. Need to Implement FFT over Spans.
        
        var wndSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        var screenPoints = ArrayPool<Point>.Shared.Rent(wndSize);
     
        var screenPointsMem = new Memory<Point>(screenPoints, 0, NumberOfPointsToDraw);

        var resampledPower = ArrayPool<float>.Shared.Rent(NumberOfPointsToDraw);
        var resPowerMem = new Memory<float>(resampledPower, 0, NumberOfPointsToDraw);
        // todo: i alread forgot that power is also complex
        SignalDecimation.ResampleData(PowerMemoryHandle.Span, resPowerMem.Span);
        
        var scaleFreq = ArrayPool<float>.Shared.Rent(NumberOfPointsToDraw);
        AcceleratedMath.FrequencyScale(scaleFreq.AsSpan(0, NumberOfPointsToDraw),
            NumberOfPointsToDraw,DrawingProperties.SamplingRate);
        
        var resampledFreq = ArrayPool<float>.Shared.Rent(NumberOfPointsToDraw);
        var resFreqMem = new Memory<float>(resampledFreq, 0, NumberOfPointsToDraw);

        SignalDecimation.ResampleData(scaleFreq.AsSpan(0, NumberOfPointsToDraw), resFreqMem.Span);
        
        var ys = resPowerMem.Span;
        var xs = resFreqMem.Span;
        
        Draw(screenPointsMem, ys, xs);
        Rendered = true;
        
        
        ArrayPool<Point>.Shared.Return(screenPoints, true); 
        ArrayPool<float>.Shared.Return(resampledPower, true);
        ArrayPool<float>.Shared.Return(resampledFreq, true);
        ArrayPool<float>.Shared.Return(scaleFreq, false);
    }

    protected override void Draw(Memory<Point> generatePoints, Span<float> ys, Span<float> xs)
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
        ReadOnlySpan<float> ys,
        ReadOnlySpan<float> xs)
    {
        for (var i = 0; i < ys.Length; i++)
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
