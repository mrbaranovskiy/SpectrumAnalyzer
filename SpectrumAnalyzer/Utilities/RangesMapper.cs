using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media;

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
    
    /// <summary>
    /// Just clamp to 0..1
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
    
    //some better way to interpolate points.
    public static (double, double) Map2Point(
        in Point data,               
        int height, int width,  
        double dbMin, double dbMax,
        double fMin,  double fMax,
        bool xLog = false)
    {
        if (width <= 0 || height <= 0) return new (0, 0);

        // some protectiopn
        if (dbMax <= dbMin)
            dbMax = dbMin + 1e-9;
        
        if (fMax <= fMin)
            fMax  = fMin + 1e-9;

        var fx = data.X;
        double nx; 

        if (!xLog)
        {
            nx = (fx - fMin) / (fMax - fMin);
        }
        else
        {
            // not used right now but would be cool to have.
            var lfMin = Math.Log(Math.Max(fMin, 1e-12));
            var lfMax = Math.Log(Math.Max(fMax, 1e-12));
            var lfx   = Math.Log(Math.Max(fx,   1e-12));
            nx = (lfx - lfMin) / (lfMax - lfMin);
        }
        nx = Clamp01(nx);
        var px = nx * (width - 1);

        var db = data.Y;
        var ny = (dbMax - db) / (dbMax - dbMin);
        ny = Clamp01(ny);
        var py = ny * (height - 1);

        return new (px, py);
    }
}
