using System.Numerics;
using Avalonia;

namespace SpectrumAnalyzer.Utilities;

/// <summary>
/// Difference kinds of range remappers.
/// </summary>
public static class RangesMapper
{
    public static double Remap(double value, double oldMin, double oldMax, double newMin, double newMax)
    {
        var normalizedValue = (value - oldMin) / (oldMax - oldMin);
        var remappedValue = newMin + (normalizedValue * (newMax - newMin));

        return remappedValue;
    }

    public static (double, double) Map2Point(
        in Point point,
        double screen_h, double screen_w,
        double y_min, double y_max,
        double x_min, double x_max,
        double factor_x = 1.0, double factor_y = 1.0)
    {
        var yr = Remap(point.Y, y_min, y_max, 0, screen_h) * factor_y;
        var xr = Remap(point.X, x_min, x_max, 0, screen_w) * factor_x;
        
        return (xr, yr);
    }
}
