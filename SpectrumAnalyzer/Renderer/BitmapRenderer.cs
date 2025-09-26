using System;
using System.Collections.Generic;

namespace SpectrumAnalyzer.Renderer;

public abstract class AbstractBitmapRenderer<TData> : IBitmapRenderer<TData> where TData : struct
{
    private readonly IStreamingDataPool<TData> _dataPool;
    private List<IRendererRepresentation> _representations;

    protected AbstractBitmapRenderer(IStreamingDataPool<TData> dataPool)
    {
        _dataPool = dataPool ?? throw new ArgumentNullException(nameof(dataPool));
    }

    public void AddRepresentation(IRendererRepresentation representation)
    {
        if (representation == null) 
            throw new ArgumentNullException(nameof(representation));
        _representations.Add(representation);
    }

    public void Render()
    {
        foreach (var r in _representations)
        {
            r.BuildRepresentation();
        }
    }
}
