using System;
using System.Numerics;

namespace SpectrumAnalyzer.Renderer;

public class WaterfallRepresentation : RendererRepresentationAbstract<WaterfallRepresentation, Complex>
{
    public WaterfallRepresentation(int singleBufferLength) : base(singleBufferLength)
    {
    }

    public override void BuildRepresentation(ReadOnlySpan<Complex> data)
    {
        throw new NotImplementedException();
    }

    public override ReadOnlySpan<byte> CurrentFrame => ReadOnlySpan<byte>.Empty;

    protected override void HandleDrawingPropertiesUpdated()
    {
        throw new NotImplementedException();
    }
}
