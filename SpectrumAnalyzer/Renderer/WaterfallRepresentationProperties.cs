namespace SpectrumAnalyzer.Renderer;

public record WaterfallRepresentationProperties(
    double Width,
    double Height,
    WaterfallColorLookup ColorLookup,
    double MinFrequency,
    double MaxFrequency,
    double SamplingRage)
{
    private readonly double _width = Width;
    private readonly double _height = Height;
    private readonly WaterfallColorLookup _colorLookup = ColorLookup;
    private readonly double _minFrequency = MinFrequency;
    private readonly double _maxFrequency = MaxFrequency;
    private readonly double _samplingRage = SamplingRage;

    public double Width
    {
        get => _width;
        init => _width = value;
    }

    public double Height
    {
        get => _height;
        init => _height = value;
    }

    public WaterfallColorLookup ColorLookup
    {
        get => _colorLookup;
        init => _colorLookup = value;
    }

    public double MinFrequency
    {
        get => _minFrequency;
        init => _minFrequency = value;
    }

    public double MaxFrequency
    {
        get => _maxFrequency;
        init => _maxFrequency = value;
    }

    public double SamplingRage
    {
        get => _samplingRage;
        init => _samplingRage = value;
    }
}
