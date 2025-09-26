
namespace SpectrumAnalyzer.Renderer;

public interface IBitmapRenderer<TData>  where TData : struct
{
    void AddRepresentation(IRendererRepresentation<TData> representation);
    void Render();
}

