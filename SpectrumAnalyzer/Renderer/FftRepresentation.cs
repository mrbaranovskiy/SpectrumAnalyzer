using System;
using System.Buffers;
using System.Numerics;
using Avalonia;
using SpectrumAnalyzer.Utilities;
using Avalonia.Media;


namespace SpectrumAnalyzer.Renderer;

public class FftRepresentation : RendererRepresentationAbstract<FFTRepresentationProperties, Complex>
{
    private BitmapGraphics _bitmapGraphics;

    public FftRepresentation(FFTRepresentationProperties properties) 
        : base(properties)
    {
       UpdateDrawingProperties(properties);
    }

    private void InitBuffers()
    {
        _bitmapGraphics = BitmapGraphics.CreateGraphics(DrawingProperties.Width, DrawingProperties.Height, 1.0);
        
        var windowSize = DrawingProperties.Height * DrawingProperties.Width * 4;
        
        _bitmapPool = ArrayPool<byte>.Create(windowSize, 1);
        _bitmapBuffer = _bitmapPool.Rent(windowSize);
        _bitmapMemoryHandle = new Memory<byte>(_bitmapBuffer, 0, windowSize);
        _signalBuffer = ArrayPool<Complex>.Shared.Rent(DrawingProperties.DataBufferLength);
        _signalMemoryHandle = new Memory<Complex>(_signalBuffer, 0, DrawingProperties.DataBufferLength);
    }

    public override void UpdateDrawingProperties(FFTRepresentationProperties properties)
    {
        if(properties.Width <= 0 || properties.Height <= 0)
            return;
        
        //do nothing if buffers size not affected.
        if(DrawingProperties.Width == properties.Width
           && DrawingProperties.Height == properties.Height 
           && DrawingProperties.DataBufferLength == properties.DataBufferLength
           )
            
            return;

        if (_bitmapBuffer != null) _bitmapPool?.Return(_bitmapBuffer);
        if (_signalBuffer != null) ArrayPool<Complex>.Shared.Return(_signalBuffer);
        
        DrawingProperties = properties;
        
        InitBuffers();

       
    }
    
    public override void Dispose()
    {
        _bitmapPool.Return(_bitmapBuffer);
        ArrayPool<Complex>.Shared.Return(_signalBuffer);
    }

    public override void BuildRepresentation(ReadOnlySpan<Complex> data)
    {
        if (data.Length != _signalMemoryHandle.Length)
            throw new NotImplementedException("Implement resize");
        
        //todo: probably this is redundant copy
        data.CopyTo(_signalMemoryHandle.Span);

        // FftSharp.Windows.Rectangular rw = new Rectangular();
        FftSharp.FFT.Forward(_signalMemoryHandle.Span);
        // todo: GC intensive code. Need to reimplement this.
        var power = FftSharp.FFT.Power(_signalBuffer);
        var freq = FftSharp.FFT.FrequencyScale(power.Length, DrawingProperties.SamplingRate);

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
        
        
        ArrayPool<Point>.Shared.Return(screenPoints, true); 
        ArrayPool<double>.Shared.Return(resampledPower, true);
        ArrayPool<double>.Shared.Return(resampledFreq, true);
    }
    
    // public void UpdateData(Bitmap bitmap, ReadOnlySpan<Point> pixels) // length = w*h*4 (premul)
    // {
    //     for (int i = 0; i < pixels.Length; i++)
   

    //     {

    //         if (pixels[i].X < 0 || pixels[i].X >= bitmap.Width || pixels[i].Y < 0 || pixels[i].Y >= bitmap.Height)

    //             continue;

    //         

    //         bitmap.SetPixel((int)pixels[i].X, (int)pixels[i].Y, Color.Blue);

    //     }        

    // }


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
        
    }
}
