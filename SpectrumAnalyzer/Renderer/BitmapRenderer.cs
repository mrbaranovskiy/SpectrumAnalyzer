using System;
using System.Collections.Generic;

namespace SpectrumAnalyzer.Renderer;

public abstract class AbstractBitmapRenderer<TData> : IBitmapRenderer<TData> where TData : struct
{
    private readonly IStreamingDataPool<TData> _dataPool;
    private List<IRendererRepresentation<TData>> _representations;

    protected AbstractBitmapRenderer(IStreamingDataPool<TData> dataPool)
    {
        _dataPool = dataPool ?? throw new ArgumentNullException(nameof(dataPool));
    }

    public void AddRepresentation(IRendererRepresentation<TData> representation)
    {
        ArgumentNullException.ThrowIfNull(representation);
        
        _representations.Add(representation);
    }

    public void Render()
    {
        var data = _dataPool.PeekLatestData();
        
        if(data.IsEmpty) 
            return;
        
        foreach (var r in _representations)
        {
            r.BuildRepresentation(data);
        }
    }
}
