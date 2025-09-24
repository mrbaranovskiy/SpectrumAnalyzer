using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SpectrumAnalyzer.Utilities;

public static class WriteableBitmapUtilities
{
    public static unsafe void FillSolid(this WriteableBitmap bmp, Color color)
    {
        using var fb = bmp.Lock();

        var w = fb.Size.Width;
        var h = fb.Size.Height;
        var rowBytes = fb.RowBytes;

        var a = color.A;
        var r = (byte)(color.R * a / 255);
        var g = (byte)(color.G * a / 255);
        var b = (byte)(color.B * a / 255);

        var packed = (uint)(b | (g << 8) | (r << 16) | (a << 24));

        for (var y = 0; y < h; y++)
        {
            var row = (uint*)((byte*)fb.Address + y * rowBytes);

            for (var x = 0; x < w; x++)
                row[x] = packed;
        }
    }
}
