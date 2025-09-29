using System;
using System.Buffers;
using System.Drawing.Imaging;
using System.Numerics;
using Avalonia;
using SpectrumAnalyzer.Utilities;
using Avalonia.Media;


namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation : RendererRepresentationAbstract<FFTDrawingProperties, Complex>
{
    public FftRepresentation(FFTDrawingProperties properties) 
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
        if (data.Length != SignalMemoryHandle.Length)
            throw new NotImplementedException("Implement resize");
        
        //todo: probably this is redundant copy
        data.CopyTo(SignalMemoryHandle.Span);
        BitmapMemoryHandle.Span.Clear();

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
        int numberOfDrawedPoints =  Math.Max(SignalBuffer.Length / 2, 1024);
        var screenPointsMem = new Memory<Point>(screenPoints, 0, numberOfDrawedPoints);

        var resampledPower = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resPowerMem = new Memory<double>(resampledPower, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(power, resPowerMem.Span);
        
        var resampledFreq = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resFreqMem = new Memory<double>(resampledFreq, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(freq, resFreqMem.Span);
        
        var ys = resPowerMem.Span;
        var xs = resFreqMem.Span;
        
        GeneratePoints(screenPointsMem.Span, ys, xs );
        
        // _bitmapGraphics.DrawLines(_bitmapMemoryHandle.Span, screenPointsMem.Span, Colors.White);
        BitmapGraphics.DrawLines(BitmapMemoryHandle, screenPointsMem, Colors.Green);
        ArrayPool<Point>.Shared.Return(screenPoints, true); 
        ArrayPool<double>.Shared.Return(resampledPower, true);
        ArrayPool<double>.Shared.Return(resampledFreq, true);
    }

    public override ReadOnlySpan<byte> CurrentFrame => BitmapMemoryHandle.Span;

    private void GeneratePoints(Span<Point> output,
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
                DrawingProperties.XAxisRange.Min,
                DrawingProperties.SamplingRate / 2
            );
            
            output[i] = new Point(scaledPt.Item1, scaledPt.Item2);
        }
    }

    protected override void HandleDrawingPropertiesUpdated()
    {
        
    }
}
