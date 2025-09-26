using System;
using System.Buffers;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation : RendererRepresentationAbstract<FFTRepresentationProperties, Complex>
{
    private readonly Graphics _graphics;

    public FftRepresentation(int singleBufferLength) : base(singleBufferLength)
    {
        _graphics = Graphics.CreateGraphics(DrawingProperties.Width, DrawingProperties.Height, 1.0);
        
        var windowSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        
        _bitmapPool = ArrayPool<byte>.Create(windowSize, 1);
        _bitmapBuffer = _bitmapPool.Rent(windowSize);
        _bitmapMemoryHandle = new Memory<byte>(_bitmapBuffer, 0, windowSize);
        _signalBuffer = ArrayPool<Complex>.Shared.Rent(singleBufferLength);
        _signalMemoryHandle = new Memory<Complex>(_signalBuffer, 0, windowSize);
    }

    public override void BuildRepresentation(ReadOnlySpan<Complex> data)
    {
        //todo: probably this is redundant copy
        data.CopyTo(_signalMemoryHandle.Span);

        // FftSharp.Windows.Rectangular rw = new Rectangular();
        FftSharp.FFT.Forward(_signalMemoryHandle.Span);
        // todo: GC intensive code. Need to reimplement this.
        var power = FftSharp.FFT.Power(_signalBuffer);
        var freq = FftSharp.FFT.FrequencyScale(power.Length, DrawingProperties.SamplingRate);

        // cut only needed frequencies, because we can zoon in/out on the screen.

        // var min = -(int)DrawingProperties.Bandwidth / 2;
        // var max = (int)DrawingProperties.Bandwidth / 2;
        // var imin = 0;
        // var imax = 0;
        //
        // //??? check it...
        // for (int i = 0; i < freq.Length; i++)
        // {
        //     if (freq[i] < min) 
        //         continue;
        //     
        //     imin = Math.Max(i - 1, 0);
        //     break;
        // }
        //
        // for (int i = freq.Length - 1; i >= 0; i--)
        // {
        //    if(freq[i] > max)
        //        continue;
        //    imax = Math.Min(i + 1, freq.Length - 1);
        // }
        
        // this cut spectrum
        //var powerSpan = new Span<double>(power, imin, imax);


        var wndSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        var screenPoints = ArrayPool<Point>.Shared.Rent(wndSize);
        var screenPointsMem = new Memory<Point>(screenPoints, 0, wndSize);
        
        //todo: need to decide how many points to draw on the screen. 
        //there is no much sense to draw them all.
        // temporary I took 3 screen width. 
        // Mayne some Shannon theorem to avoid signal lost.
        int numberOfDrawedPoints = DrawingProperties.Width * 3;

        var resampledPower = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resPowerMem = new Memory<double>(resampledPower, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(power, resPowerMem.Span);
        
        var resampledFreq = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resFreqMem = new Memory<double>(resampledFreq, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(freq, resFreqMem.Span);
        
        var ys = resPowerMem.Span;
        var xs = resFreqMem.Span;
        
        GeneratePoints(screenPointsMem.Span, ys, xs );
        
        _graphics.DrawLines(_bitmapMemoryHandle.Span, screenPointsMem.Span, Colors.White);
        
        ArrayPool<Point>.Shared.Return(screenPoints); 
        ArrayPool<double>.Shared.Return(resampledPower);
        ArrayPool<double>.Shared.Return(resampledFreq);

        //return _bitmapMemoryHandle.Span;
    }

    public override ReadOnlySpan<byte> CurrentFrame => _bitmapMemoryHandle.Span;

    private void GeneratePoints(Span<Point> output,
        ReadOnlySpan<double> ys,
        ReadOnlySpan<double> xs)
    {
        for (int i = 0; i < ys.Length; i++)
        {
            var complex = new Complex(ys[i], xs[i]);
            var scaledPt = RangesMapper.Map2Point(complex, 
                DrawingProperties.Width,
                DrawingProperties.Height,
                DrawingProperties.YAxisRange.Min,
                DrawingProperties.YAxisRange.Max,
                DrawingProperties.XAxisRange.Min,
                DrawingProperties.XAxisRange.Max
            );
            
            output[i] = new Point(scaledPt.Item1, scaledPt.Item2);
        }
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
