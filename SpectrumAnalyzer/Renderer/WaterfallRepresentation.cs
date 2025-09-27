using System;
using System.Numerics;

namespace SpectrumAnalyzer.Renderer;

public class WaterfallRepresentation : RendererRepresentationAbstract<WaterfallRepresentation, Complex>
{
    public WaterfallRepresentation(int singleBufferLength) : base(null)
    {
    }

    public override void UpdateDrawingProperties(WaterfallRepresentation properties)
    {
        throw new NotImplementedException();
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
