using System;
using System.Drawing;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Utilities;
using Brushes = Avalonia.Media.Brushes;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;
using Vector = Avalonia.Vector;

namespace SpectrumAnalyzer.Controls;

public class SignalPlot : TemplatedControl
{
    private WriteableBitmap _source;

    public SignalPlot()
    {
        SizeChanged += (sender, args) =>
        {
            ViewportHeight = (int)Height;
            ViewportWidth = (int)Width;

            if (sender is TemplatedControl { Height: > 0, Width: > 0 } ctrl)
                _source = CreateBitmap(ctrl);
        };

        Loaded += (sender, args) =>
        {
            if (sender is TemplatedControl { Height: Double.NaN, Width: Double.NaN } ctrl)
            {
                Height = ctrl.Bounds.Height;
                Width = ctrl.Bounds.Width;
            }
            
            ViewportHeight = (int)Height;
            ViewportWidth = (int)Width;

           _source = CreateBitmap((TemplatedControl)sender);
        };

        TemplateApplied += (sender, args) =>
        {
            ViewportHeight = (int)Height;
            ViewportWidth = (int)Width;


            if (sender is TemplatedControl { Height: > 0, Width: > 0 } ctrl)
                _source = CreateBitmap(ctrl);

        };
    }

    private WriteableBitmap CreateBitmap(TemplatedControl ctrl)
    {
        var width = (int)ctrl.Width;
        var height = (int)ctrl.Height;
        return new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Premul);       
    }

    private int _viewportWidth;

    public static readonly DirectProperty<SignalPlot, int> ViewportWidthProperty = AvaloniaProperty.RegisterDirect<SignalPlot, int>(
        nameof(ViewportWidth), o => o.ViewportWidth, (o, v) => o.ViewportWidth = v);

    public int ViewportWidth
    {
        get => _viewportWidth;
        set => SetAndRaise(ViewportWidthProperty, ref _viewportWidth, value);
    }

    private int _viewportHeight;

    public static readonly DirectProperty<SignalPlot, int> ViewportHeightProperty = AvaloniaProperty.RegisterDirect<SignalPlot, int>(
        nameof(ViewportHeight), o => o.ViewportHeight, (o, v) => o.ViewportHeight = v);

    public int ViewportHeight
    {
        get => _viewportHeight;
        set => SetAndRaise(ViewportHeightProperty, ref _viewportHeight, value);
    }
    
    public static readonly StyledProperty<double> MinXProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(MinX), 0);

    public static readonly StyledProperty<double> MaxXProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(MaxX), 1);

    public static readonly StyledProperty<double> MinYProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(MinY), -1);

    public static readonly StyledProperty<double> MaxYProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(MaxY), 1);

    // Grid divisions (count of cells)

    public static readonly StyledProperty<int> GridDivisionsXProperty =
        AvaloniaProperty.Register<SignalPlot, int>(nameof(GridDivisionsX), 10);

    public static readonly StyledProperty<int> GridDivisionsYProperty =
        AvaloniaProperty.Register<SignalPlot, int>(nameof(GridDivisionsY), 8);

    // Styling knobs for overlay

    public static readonly StyledProperty<IBrush> GridBrushProperty =
        AvaloniaProperty.Register<SignalPlot, IBrush>(nameof(GridBrush), Brushes.Gray);

    public static readonly StyledProperty<double> GridThicknessProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(GridThickness), 1);

    public static readonly StyledProperty<IBrush> AxisBrushProperty =
        AvaloniaProperty.Register<SignalPlot, IBrush>(nameof(AxisBrush), Brushes.White);

    public static readonly StyledProperty<double> AxisThicknessProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(AxisThickness), 1.5);

    public static readonly StyledProperty<IBrush> LabelBrushProperty =
        AvaloniaProperty.Register<SignalPlot, IBrush>(nameof(LabelBrush), Brushes.White);

    public static readonly StyledProperty<Thickness> PlotPaddingProperty =
        AvaloniaProperty.Register<SignalPlot, Thickness>(nameof(PlotPadding), new Thickness(32, 16, 16, 28));
    
    
    public static readonly StyledProperty<IRendererRepresentation<Complex>> RepresentationProperty = AvaloniaProperty.Register<SignalPlot, IRendererRepresentation<Complex>>(
        nameof(Representation));

    public IRendererRepresentation<Complex> Representation
    {
        get => GetValue(RepresentationProperty);
        set => SetValue(RepresentationProperty, value);
    }

    public double MinX { get => GetValue(MinXProperty); set => SetValue(MinXProperty, value); }
    public double MaxX { get => GetValue(MaxXProperty); set => SetValue(MaxXProperty, value); }
    public double MinY { get => GetValue(MinYProperty); set => SetValue(MinYProperty, value); }
    public double MaxY { get => GetValue(MaxYProperty); set => SetValue(MaxYProperty, value); }

    public int GridDivisionsX { get => GetValue(GridDivisionsXProperty); set => SetValue(GridDivisionsXProperty, value); }
    public int GridDivisionsY { get => GetValue(GridDivisionsYProperty); set => SetValue(GridDivisionsYProperty, value); }

    public IBrush GridBrush { get => GetValue(GridBrushProperty); set => SetValue(GridBrushProperty, value); }
    public double GridThickness { get => GetValue(GridThicknessProperty); set => SetValue(GridThicknessProperty, value); }

    public IBrush AxisBrush { get => GetValue(AxisBrushProperty); set => SetValue(AxisBrushProperty, value); }
    public double AxisThickness { get => GetValue(AxisThicknessProperty); set => SetValue(AxisThicknessProperty, value); }

    public IBrush LabelBrush { get => GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }
    public Thickness PlotPadding { get => GetValue(PlotPaddingProperty); set => SetValue(PlotPaddingProperty, value); }
    public override void Render(DrawingContext context)
    {
        if(_source is null)
            return;
        
        UpdateData(Representation.CurrentFrame);
        var src = new Rect(-PlotPadding.Left,  PlotPadding.Bottom, Width - PlotPadding.Right , Height - PlotPadding.Top);
        var dst = new Rect(0,  0, Width  - PlotPadding.Right , Height - PlotPadding.Top);
        context.DrawImage(_source, src, dst);
    }

    private unsafe void UpdateData(ReadOnlySpan<byte> bgra)
    {
        using var fb = _source.Lock();
        var dst = new Span<byte>((void*)fb.Address, fb.RowBytes * fb.Size.Height);
        dst.Clear();
        
        
        var srcStride = (int)Width * 4;
        if (fb.RowBytes == srcStride)
        {
            bgra.CopyTo(dst);
        }
        else
        {
            for (var y = 0; y < (int)Height; y++)
            {
                bgra.Slice(y * srcStride, srcStride)
                    .CopyTo(dst.Slice(y * fb.RowBytes, srcStride));
            }
        }
    }
}

