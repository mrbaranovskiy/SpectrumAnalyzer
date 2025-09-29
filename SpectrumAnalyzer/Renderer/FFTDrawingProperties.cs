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
}

public record FFTDrawingProperties(
    int DataBufferLength,
    int Width,
    int Height,
    int Bandwidth,
    int CenterFrequency,
    int SamplingRate,
    AxisRange XAxisRange,
    AxisRange YAxisRange,
    double XScaleFrequency = 1.0, // zoom in to frequency.
    double XScale = 1.0, //todo: change it to something [0.1 .. 1.0]
    double YScale = 1.0) : IDrawingProperties;
