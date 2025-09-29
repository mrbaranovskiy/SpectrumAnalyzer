using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

public interface IDrawingProperties
{
    int DataBufferLength { get; init; }
    int Width { get; init; }
    int Height { get; init; }
    int Bandwidth { get; init; }
    int CenterFrequency { get; init; }
    int SamplingRate { get; init; }
    AxisRange XAxisRange { get; init; }
    AxisRange YAxisRange { get; init; }
    double XScaleFrequency { get; init; }
}