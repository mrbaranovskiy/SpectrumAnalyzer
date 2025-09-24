using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SpectrumAnalyzer.Controls;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly WaterfallControl? _ctrl;
    private double _time = 0;
    private double _timeDelta = 0.05;
    private const int SizeOfChunk = 4096;
    private const int NumberOfDisplayPoint = 512;
    private readonly Memory<double> _spectrumPool;
    private readonly Memory<double> _displayPointsPool;
    private readonly Memory<byte> _bitmapDataPool;
    private readonly Graphics _graphUtils;

    public MainWindow()
    {
        var signalDataPool = ArrayPool<double>.Shared.Rent(SizeOfChunk);
        var displayPoint = ArrayPool<double>.Shared.Rent(NumberOfDisplayPoint);
        
        _spectrumPool = new Memory<double>(signalDataPool, 0, SizeOfChunk);
        _displayPointsPool = new Memory<double>(displayPoint, 0, NumberOfDisplayPoint);
        
        InitializeComponent();
        
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Normal, HandleDispatcherTimerCallback);
        _ctrl = this.FindControl<WaterfallControl>("waterfall");
        _graphUtils = Graphics.CreateGraphics(_ctrl.WidthPx, _ctrl.HeightPx, 1.0);
        
        var btmPool = ArrayPool<byte>.Shared.Rent(_ctrl.WidthPx * _ctrl.HeightPx * 4);
        _bitmapDataPool = new Memory<byte>(btmPool, 0, _ctrl.WidthPx * _ctrl.HeightPx * 4);
    }

    private void HandleDispatcherTimerCallback(object? sender, EventArgs e)
    {
        _time += _timeDelta;
        _displayPointsPool.Span.Clear();
        
        FillRandomData();
        
        SignalDecimation.ResampleData(_spectrumPool.Span, _displayPointsPool.Span);
        
        var pointsPool = ArrayPool<Point>.Shared.Rent(NumberOfDisplayPoint);
        var pointsSpan = new Span<Point>(pointsPool, 0, NumberOfDisplayPoint);
        
        for (var i = 0; i < NumberOfDisplayPoint; i++)
        {
            var x = _displayPointsPool.Span[i];
            var im = new Complex((double)i / NumberOfDisplayPoint, x);
            
            var scaledPt = RangesMapper.Map2Point(im, 
                _ctrl.WidthPx, 
                _ctrl.HeightPx, 
                -1.0, 1.0, 0, 0.5); // we need to decimate. values for displaying.
            
           pointsSpan[i] = new Point(scaledPt.Item1,  scaledPt.Item2);
        }
        
        _graphUtils.DrawLines(_bitmapDataPool.Span, pointsSpan, Color.FromArgb(128, 255, 0, 0));
        _ctrl.UpdateData(_bitmapDataPool.Span);
        _bitmapDataPool.Span.Clear();
        ArrayPool<Point>.Shared.Return(pointsPool);
        
    }
    
    private void FillRandomData()
    {
        var delta = _timeDelta / SizeOfChunk;
        double accumulated = 0;
        for (var i = 0; i < _spectrumPool.Span.Length; i++)
        {
            _spectrumPool.Span[i] = (Random.Shared.NextSingle()) - 1;
            accumulated+=delta;
        }        
    }
}

