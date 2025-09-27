
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.Renderer;

public interface IBitmapRenderer<TData> : IDataReady  where TData : struct
{
    void AddRepresentation(IRendererRepresentation<TData> representation);
    void Render();
}

