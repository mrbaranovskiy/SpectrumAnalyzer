using System;
using System.Runtime.CompilerServices;
using Avalonia;

namespace SpectrumAnalyzer.Utilities;

public static class PointExtensions
{
    public static double Distance(this Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PosInBuffer(this Point point, int w, int h) => (w * (int)point.Y + (int)point.X) * 4;
}
