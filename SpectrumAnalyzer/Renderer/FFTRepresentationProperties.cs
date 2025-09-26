using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

public record FFTRepresentationProperties(
    int Width,
    int Height,
    double Bandwidth,
    double CenterFrequency,
    double SamplingRate,
    AxisRange XAxisRange,
    AxisRange YAxisRange,
    double XScaleFrequency, // zoom in to frequency.
    double XScale = 1.0, //todo: change it to something [0.1 .. 1.0]
    double YScale = 1.0);
