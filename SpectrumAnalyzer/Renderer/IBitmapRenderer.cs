using System;
using System.Collections.Generic;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;
//не три різних рендерера, а три різних репрезентації.
//інакше кожен буде забирати свій чанк
public interface IBitmapRenderer<TData>  where TData : struct
{
    void UpdateData(ReadOnlySpan<TData> data);
    void AddRepresentation();
    void Render();
}

public interface IStreamingDataPool<TData> 
    : IDataReceived<DataReceivedEventArgs>,
        IDisposable where TData : struct
{
    bool IsAvailable { get; }
    bool RequestLatest(Span<float> buffer);
    int RequestLatestDataLength();
}


// 
