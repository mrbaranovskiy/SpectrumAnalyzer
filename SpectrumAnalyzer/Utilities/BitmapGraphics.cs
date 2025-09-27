using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;

namespace SpectrumAnalyzer.Utilities;

public sealed class BitmapGraphics
{
    private readonly int _width;
    private readonly int _height;
    private readonly double _factor;

    private BitmapGraphics(int width, int height, double factor)
    {
        _width = width;
        _height = height;
        _factor = factor;
    }
    
    public static BitmapGraphics CreateGraphics(int width, int height, double factor)
    {
        return new BitmapGraphics(width, height, factor);
    }
    
    private static int Round(float n)
    {
        if (n - (int)n < 0.5)
            return (int)n;
        return (int)(n + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void DrawLine(Span<byte> image, Point p0, Point p1, uint color)
    {
        var dx = (int)(p1.X - p0.X);
        var dy = (int)(p1.Y - p0.Y);

        var step = Math.Abs(Math.Abs(dx) > Math.Abs(dy) ? dx : dy);

        var x_incr = (float)dx / step;
        var y_incr = (float)dy / step;
        
        var x = (float)p0.X;
        var y = (float)p0.Y;
        
        for (var i = 0; i < step; i++) {

            var nextPoint = new Point(Round(x), Round(y));
            var position = nextPoint.PosInBuffer(_width, _height);

            if (position < image.Length && position > 0)
            {
                image[position + 3] = (byte)((color & 0xFF000000) >> 24); // A
                image[position + 0] = (byte)((color & 0x00FF0000) >> 16); //  R
                image[position + 1] = (byte)((color & 0x0000FF00) >> 8);  // G
                image[position + 2] = (byte)(color & 0x000000FF);  // B
            }
            
            // should we increment anyway??
            x += x_incr;
            y += y_incr;
        }
    }

    public void DrawLines(Span<byte> image, ReadOnlySpan<Point> points, Color color)
    {
        for (var i = 1; i < points.Length; i++)
        {
            var p1 = points[i - 1];
            var p2 = points[i];
           
            DrawLine(image, p1, p2, color.ToUInt32());
        }
    }
}
