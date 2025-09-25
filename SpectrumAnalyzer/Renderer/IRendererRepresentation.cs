using System;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Renderer;
// time/fft/waterfall
public interface IRendererRepresentation<TDrawingProperties, TData>
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
    private readonly IStreamingDataPool<TData> _dataPool;

    protected RendererRepresentationAbstract(IStreamingDataPool<TData> dataPool)
    {
        _dataPool = dataPool;
    }
 
    protected TDrawingProperties DrawingProperties { get; set; }
    public abstract ReadOnlySpan<byte> BuildRepresentation();
}


