using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace SpectrumAnalyzer.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        // stupid way...
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(25), DispatcherPriority.Render,
            (_, args) =>
            {
                // WaterflowPlotChart.InvalidateVisual();
                // SignalPlotChart.InvalidateVisual();
            });
    }
}