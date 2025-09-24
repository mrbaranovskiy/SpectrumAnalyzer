using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SpectrumAnalyzer.Controls;

public class WaterfallControl : Control
{
    public WriteableBitmap Bitmap { get; }
    public int WidthPx  { get; }
    public int HeightPx { get; }

    public WaterfallControl() : this(1000, 500)
    {
        this.Name = "waterfall";
    }
    
    public WaterfallControl(int w, int h)
    {
        WidthPx = w; HeightPx = h;
        Bitmap = new WriteableBitmap(
            new PixelSize(w, h),
            new Vector(96, 96),                  // DPI
            PixelFormat.Rgba8888,
            AlphaFormat.Premul);                 // important for speed
    }

    public unsafe void UpdateData(ReadOnlySpan<byte> bgra) // length = w*h*4 (premul)
    {
        using var fb = Bitmap.Lock();            // ILockedFramebuffer
        var dst = new Span<byte>((void*)fb.Address, fb.RowBytes * fb.Size.Height);
        

        int srcStride = WidthPx * 4;
        if (fb.RowBytes == srcStride)
        {
            bgra.CopyTo(dst);
        }
        else
        {
            // handle padded rowbytes
            for (int y = 0; y < HeightPx; y++)
            {
                bgra.Slice(y * srcStride, srcStride)
                    .CopyTo(dst.Slice(y * fb.RowBytes, srcStride));
            }
        }

        InvalidateVisual();                      // ask Avalonia to redraw us
    }

    public override void Render(DrawingContext ctx)
    {
        if (Bitmap is null) return;
        var src = new Rect(0, 0, WidthPx, HeightPx);
        var dst = new Rect(Bounds.Size);
        ctx.DrawImage(Bitmap, src, dst);
    }
}
