using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace SpectrumAnalyzer.Controls;

internal class SignalPlotOverlay : Control
{
    // Bind-through properties (come from SignalPlot via TemplateBinding)
    public static readonly StyledProperty<double> MinXProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(MinX), 0);
    public static readonly StyledProperty<double> MaxXProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(MaxX), 1);
    public static readonly StyledProperty<double> MinYProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(MinY), -1);
    public static readonly StyledProperty<double> MaxYProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(MaxY), 1);

    public static readonly StyledProperty<int> GridDivisionsXProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, int>(nameof(GridDivisionsX), 10);
    public static readonly StyledProperty<int> GridDivisionsYProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, int>(nameof(GridDivisionsY), 8);

    public static readonly StyledProperty<IBrush> GridBrushProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, IBrush>(nameof(GridBrush), Brushes.Gray);
    public static readonly StyledProperty<double> GridThicknessProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(GridThickness), 1);

    public static readonly StyledProperty<IBrush> AxisBrushProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, IBrush>(nameof(AxisBrush), Brushes.White);
    public static readonly StyledProperty<double> AxisThicknessProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, double>(nameof(AxisThickness), 1.5);

    public static readonly StyledProperty<IBrush> LabelBrushProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, IBrush>(nameof(LabelBrush), Brushes.White);

    public static readonly StyledProperty<Thickness> PlotPaddingProperty =
        AvaloniaProperty.Register<SignalPlotOverlay, Thickness>(nameof(PlotPadding), new Thickness(32,16,16,28));

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

    private static readonly Typeface Typeface = new(new FontFamily("Segoe UI"), FontStyle.Normal, FontWeight.Normal);

    public SignalPlotOverlay()
    {
        AffectsRender<SignalPlotOverlay>(
            MinXProperty, MaxXProperty, MinYProperty, MaxYProperty,
            GridDivisionsXProperty, GridDivisionsYProperty,
            GridBrushProperty, GridThicknessProperty,
            AxisBrushProperty, AxisThicknessProperty,
            LabelBrushProperty, PlotPaddingProperty);
    }

    public override void Render(DrawingContext ctx)
    {
        var r = Bounds;
        if (r.Width <= 0 || r.Height <= 0)
            return;

        var inner = new Rect(
            r.X + PlotPadding.Left,
            r.Y + PlotPadding.Top,
            Math.Max(0, r.Width - PlotPadding.Left - PlotPadding.Right),
            Math.Max(0, r.Height - PlotPadding.Top - PlotPadding.Bottom));

        if (inner.Width > 0 && inner.Height > 0)
        {
            var gridPen = new Pen(GridBrush, GridThickness);
            var axisPen = new Pen(AxisBrush, AxisThickness);

            if (GridDivisionsX > 0)
            {
                for (int i = 0; i <= GridDivisionsX; i++)
                {
                    double x = inner.X + inner.Width * i / GridDivisionsX;
                    ctx.DrawLine(gridPen, new Point(x, inner.Top), new Point(x, inner.Bottom));
                }
            }

            if (GridDivisionsY > 0)
            {
                for (int j = 0; j <= GridDivisionsY; j++)
                {
                    double y = inner.Y + inner.Height * j / GridDivisionsY;
                    ctx.DrawLine(gridPen, new Point(inner.Left, y), new Point(inner.Right, y));
                }
            }

            ctx.DrawLine(axisPen, new Point(inner.Left, inner.Top), new Point(inner.Right, inner.Top));
            ctx.DrawLine(axisPen, new Point(inner.Left, inner.Bottom), new Point(inner.Right, inner.Bottom));
            ctx.DrawLine(axisPen, new Point(inner.Left, inner.Top), new Point(inner.Left, inner.Bottom));
            ctx.DrawLine(axisPen, new Point(inner.Right, inner.Top), new Point(inner.Right, inner.Bottom));

            int xTickCount = GridDivisionsX;
            int yTickCount = GridDivisionsY;

            for (int i = 0; i <= xTickCount; i++)
            {
                double t = (double)i / xTickCount;
                double x = inner.X + t * inner.Width;

                ctx.DrawLine(axisPen, new Point(x, inner.Bottom), new Point(x, inner.Bottom + 4));

                double xv = MinX + t * (MaxX - MinX);
                DrawLabel(ctx, $"{xv:0.###}", new Point(x, inner.Bottom + 6), hAlignCenter: true, vAlignTop: true);
            }

            for (int j = 0; j <= yTickCount; j++)
            {
                double t = (double)j / yTickCount;
                double y = inner.Y + (1 - t) * inner.Height;

                ctx.DrawLine(axisPen, new Point(inner.Left - 4, y), new Point(inner.Left, y));

                double yv = MinY + t * (MaxY - MinY);
                DrawLabel(ctx, $"{yv:0.###}", new Point(inner.Left - 6, y), hAlignRight: true, vAlignCenter: true);
            }
        }
    }

    private void DrawLabel(
        DrawingContext ctx,
        string text,
        Point anchor,
        bool hAlignCenter = false,
        bool hAlignRight = false,
        bool vAlignTop = false,
        bool vAlignCenter = false)
    {
        var layout = new TextLayout(text, Typeface, 12, LabelBrush);
        double x = anchor.X, y = anchor.Y;

        if (hAlignCenter) x -= layout.Width / 2;
        else if (hAlignRight) x -= layout.Width;

        if (vAlignCenter) y -= layout.Height / 2;
        else if (vAlignTop) {}
        else y -= layout.Height;

        layout.Draw(ctx, new Point(x, y));
    }
}
