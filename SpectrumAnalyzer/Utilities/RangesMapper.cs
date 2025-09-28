using System;
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

    // public static (double, double) Map2Point(
    //     in Point point,
    //     double screen_h, double screen_w,
    //     double y_min, double y_max,
    //     double x_min, double x_max,
    //     double factor_x = 1.0, double factor_y = 1.0)
    // {
    //     var yr = Remap(point.Y, y_min, y_max, 0, screen_h) * factor_y;
    //     var xr = Remap(point.X, x_min, x_max, 0, screen_w) * factor_x;
    //     
    //     return (xr, yr);
    // }
    
    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    public static (double, double) Map2Point(
        in Point data,                 // data.X = frequency (Hz), data.Y = magnitude (dB)
        int height, int width,  // control size in pixels
        double dbMin, double dbMax,
        double fMin,  double fMax,
        bool xLog = false)
    {
        // Guard size
        if (width <= 0 || height <= 0) return new (0, 0);

        if (dbMax <= dbMin) { dbMax = dbMin + 1e-9; }
        if (fMax <= fMin)   { fMax  = fMin + 1e-9; }

        double fx = data.X;
        double nx; // normalized 0..1

        if (!xLog)
        {
            nx = (fx - fMin) / (fMax - fMin);
        }
        else
        {
            // Log axis needs strictly positive bounds
            double lfMin = Math.Log(Math.Max(fMin, 1e-12));
            double lfMax = Math.Log(Math.Max(fMax, 1e-12));
            double lfx   = Math.Log(Math.Max(fx,   1e-12));
            nx = (lfx - lfMin) / (lfMax - lfMin);
        }
        nx = Clamp01(nx);
        double px = nx * (width - 1);

        double db = data.Y;
        double ny = (dbMax - db) / (dbMax - dbMin); // 0 at top, 1 at bottom
        ny = Clamp01(ny);
        double py = ny * (height - 1);

        return new (px, py);
    }
}
