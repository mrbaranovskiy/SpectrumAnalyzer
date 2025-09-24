using System;
using System.Collections.Generic;

namespace SpectrumAnalyzer.Renderer;
//не три різних рендерера, а три різних репрезентації.
//інакше кожен буде забирати свій чанк
public interface IBitmapRenderer<TData> where TData : struct
{
    void UpdateData(ReadOnlySpan<TData> data);
    void Render();
}

public interface IStreamingDataPool<TData> : IDisposable where TData : struct
{
    bool IsAvailable { get; }
    bool RequestLatest(Span<float> buffer);
    int RequestLatestDataLength();
}
