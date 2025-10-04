using System;
using System.Collections.Generic;
using Avalonia.Media;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.Renderer;

public abstract class AbstractBitmapRenderer<TData> 
    : IBitmapRenderer<TData> where TData : struct
{
    private readonly IStreamingDataPool<TData> _dataPool;
    private readonly List<IRendererRepresentation<TData>> _representations;

    protected AbstractBitmapRenderer(IStreamingDataPool<TData> dataPool)
    {
        _dataPool = dataPool ?? throw new ArgumentNullException(nameof(dataPool));
        _representations = new List<IRendererRepresentation<TData>>(3);
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
        //todo: make them async.
        
        foreach (var r in _representations)
            r.BuildRepresentation(data);
        
        OnDataReady();
        
        _dataPool.ReleaseLatestData();
    }

    public event EventHandler<EventArgs>? DataReady;

    protected virtual void OnDataReady()
    {
        DataReady?.Invoke(this, EventArgs.Empty);
    }
}

public class ComplexDataRenderer(IStreamingDataPool<ComplexF> dataPool)
    : AbstractBitmapRenderer<ComplexF>(dataPool);

public class FloatDataRenderer(IStreamingDataPool<float> dataPool)
    : AbstractBitmapRenderer<float>(dataPool);

