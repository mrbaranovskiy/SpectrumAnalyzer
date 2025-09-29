using System;
using System.Numerics;

namespace SpectrumAnalyzer.Renderer;

public sealed class WaterfallRepresentation : RendererRepresentationAbstract<WaterfallDrawingProperties, Complex>
{
    private byte[] _cyclicScreenBuffer = null;
    private Memory<byte> _cyclicScreenBufferMemory = null;
    private int _cycleIndex = 0;
    
    public WaterfallRepresentation(WaterfallDrawingProperties properties) : base(properties)
    {
        UpdateDrawingProperties(properties);
    }

    public override void InitBuffers()
    {
        if(_cyclicScreenBuffer != null)
            BitmapPool.Return(_cyclicScreenBuffer);
        
        _cyclicScreenBuffer = this.BitmapPool.Rent(
            DrawingProperties.Width * DrawingProperties.Height * 4);
        
        base.InitBuffers();
    }

    public override void BuildRepresentation(ReadOnlySpan<Complex> data)
    {
        if (data.Length != SignalMemoryHandle.Length)
            throw new NotImplementedException("Implement resize");
        
        data.CopyTo(SignalMemoryHandle.Span);
        BitmapMemoryHandle.Span.Clear();
        
        FftSharp.FFT.Forward(SignalMemoryHandle.Span);
        // todo: GC intensive code. Need to Implement FFT over Spans.
        var power = FftSharp.FFT.Power(SignalBuffer);
        var freq = FftSharp.FFT.FrequencyScale(power.Length, DrawingProperties.SamplingRate);
    }

    public override ReadOnlySpan<byte> CurrentFrame => ReadOnlySpan<byte>.Empty;

    protected override void HandleDrawingPropertiesUpdated()
    {
        throw new NotImplementedException();
    }
}
