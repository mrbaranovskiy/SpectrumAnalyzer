using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Renderer;

public record WaterfallDrawingProperties(
    int DataBufferLength,
    int Width,
    int Height,
    int Bandwidth,
    int CenterFrequency,
    int SamplingRate, 
    AxisRange XRange
    ) : IDrawingProperties;