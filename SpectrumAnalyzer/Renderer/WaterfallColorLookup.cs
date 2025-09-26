using Avalonia.Media;

namespace SpectrumAnalyzer.Renderer;

public record WaterfallColorLookup(Color Min,  Color Max)
{
    private readonly Color _min = Min;
    private readonly Color _max = Max;

    public Color Min
    {
        get => _min;
        init => _min = value;
    }

    public Color Max
    {
        get => _max;
        init => _max = value;
    }
}
