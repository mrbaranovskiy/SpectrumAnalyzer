using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using Vector = Avalonia.Vector;

namespace SpectrumAnalyzer.Controls;

public class SignalPlot : TemplatedControl
{
    public SignalPlot()
    {
        SizeChanged += (sender, args) =>
        {
            ViewportHeight = (int)Height;
            ViewportWidth = (int)Width;
            
            if (sender is TemplatedControl {Height: > 0 , Width: > 0} ctrl)
                Source = CreateBitmap(ctrl);
        };

        Loaded += (sender, args) =>
        {
            ViewportHeight = (int)Height;
            ViewportWidth = (int)Width;
          
            
            if (sender is TemplatedControl {Height: > 0 , Width: > 0} ctrl )
                Source = CreateBitmap(ctrl);
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


    private IDataReady _renderSource;

    public static readonly DirectProperty<SignalPlot, IDataReady> RenderSourceProperty = AvaloniaProperty.RegisterDirect<SignalPlot, IDataReady>(
        nameof(RenderSource), o => o.RenderSource, setter: (o, v) =>
        {
            o.RenderSource = v;
        });

    public IDataReady RenderSource
    {
        get => _renderSource;
        set => SetAndRaise(RenderSourceProperty, ref _renderSource, value);
    }

    // Bottom layer: bind your WriteableBitmap (or any IBitmap)
    //
    public static readonly StyledProperty<WriteableBitmap?> SourceProperty =
        AvaloniaProperty.Register<SignalPlot, WriteableBitmap?>(nameof(Source));

    // Axes range for labels (overlay doesn’t care about how you draw pixels)
    
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
    //
    public WriteableBitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
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
        if(Representation is null || Source is null)
            return;
        
        UpdateData(Representation.CurrentFrame);

        var src = new Rect(0, 0, Width, Height);
        var dst = new Rect(Bounds.Size);
        
        context.DrawImage(Source, src, dst);
    }
    
    public unsafe void UpdateData(ReadOnlySpan<byte> bgra) // length = w*h*4 (premul)
    {
        using var fb = Source.Lock();            // ILockedFramebuffer
        var dst = new Span<byte>((void*)fb.Address, fb.RowBytes * fb.Size.Height);
        dst.Clear();
        
        var srcStride = (int)Width * 4;
        if (fb.RowBytes == srcStride)
        {
            bgra.CopyTo(dst);
        }
        else
        {
            // handle padded rowbytes
            for (var y = 0; y < (int)Height; y++)
            {
                bgra.Slice(y * srcStride, srcStride)
                    .CopyTo(dst.Slice(y * fb.RowBytes, srcStride));
            }
        }

        //InvalidateVisual();                      // ask Avalonia to redraw us
    }
}

