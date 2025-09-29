using System;
using System.Buffers;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SpectrumAnalyzer.Controls;
using SpectrumAnalyzer.Utilities;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        // stupid way...
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(25), DispatcherPriority.Render,
            (sender, args) => SignalPlotChart.InvalidateVisual());
    }
}