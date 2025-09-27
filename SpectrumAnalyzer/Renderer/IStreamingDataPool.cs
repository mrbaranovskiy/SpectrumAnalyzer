using System;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public interface IStreamingDataPool<TData> 
    : IDataReceived<DataReceivedEventArgs>,
        IDisposable where TData : struct
{
    bool IsAvailable { get; }
    bool RequestLatestCopy(Span<TData> buffer);
    ReadOnlySpan<TData> PeekLatestData();
    void ReleaseLatestData();
    int RequestLatestDataLength();
    int MaxQueueSize { get; set; }
    
}
