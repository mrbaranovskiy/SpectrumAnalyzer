
using System;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public interface IBitmapRenderer<TData> : IDataReady  where TData : struct
{
    void AddRepresentation(IRendererRepresentation<TData,ReadOnlyMemory<byte>> representation);
    void Render();
}

