using System;
using System.Buffers;
using System.Linq;
using System.Numerics;
using Avalonia;
using SpectrumAnalyzer.Utilities;
using Vector = System.Numerics.Vector;

using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;


namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation : RendererRepresentationAbstract<FFTRepresentationProperties, Complex>
{
    private readonly BitmapGraphics _bitmapGraphics;

    public FftRepresentation(FFTRepresentationProperties properties, int singleBufferLength) 
        : base(properties, singleBufferLength)
    {
        _bitmapGraphics = BitmapGraphics.CreateGraphics(DrawingProperties.Width, DrawingProperties.Height, 1.0);
        
        var windowSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        
        _bitmapPool = ArrayPool<byte>.Create(windowSize, 1);
        _bitmapBuffer = _bitmapPool.Rent(windowSize);
        _bitmapMemoryHandle = new Memory<byte>(_bitmapBuffer, 0, windowSize);
        _signalBuffer = ArrayPool<Complex>.Shared.Rent(singleBufferLength);
        _signalMemoryHandle = new Memory<Complex>(_signalBuffer, 0, singleBufferLength);
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
        int numberOfDrawedPoints = _signalBuffer.Length / 2;

        var resampledPower = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resPowerMem = new Memory<double>(resampledPower, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(power, resPowerMem.Span);
        
        var resampledFreq = ArrayPool<double>.Shared.Rent(numberOfDrawedPoints);
        var resFreqMem = new Memory<double>(resampledFreq, 0, numberOfDrawedPoints);
        SignalDecimation.ResampleData(freq, resFreqMem.Span);
        
        var ys = resPowerMem.Span;
        var xs = resFreqMem.Span;
        
        GeneratePoints(screenPointsMem.Span, ys, xs );
        
        _bitmapGraphics.DrawLines(_bitmapMemoryHandle.Span, screenPointsMem.Span, Colors.White);
        //var btm = new Bitmap(DrawingProperties.Width, DrawingProperties.Height, PixelFormat.Format32bppRgb);
        //UpdateData(btm, screenPointsMem.Span);
        //btm.Save("D:\\fft.bmp");

        // var cnt = _bitmapMemoryHandle.Span.ToArray().Count(s => s > 0);
        
        ArrayPool<Point>.Shared.Return(screenPoints); 
        ArrayPool<double>.Shared.Return(resampledPower);
        ArrayPool<double>.Shared.Return(resampledFreq);

        //return _bitmapMemoryHandle.Span;
    }
    
    public void UpdateData(Bitmap bitmap, ReadOnlySpan<Point> pixels) // length = w*h*4 (premul)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].X < 0 || pixels[i].X >= bitmap.Width || pixels[i].Y < 0 || pixels[i].Y >= bitmap.Height)
                continue;
            
            bitmap.SetPixel((int)pixels[i].X, (int)pixels[i].Y, Color.Blue);
        }        
    }

    public override ReadOnlySpan<byte> CurrentFrame => _bitmapMemoryHandle.Span;

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
