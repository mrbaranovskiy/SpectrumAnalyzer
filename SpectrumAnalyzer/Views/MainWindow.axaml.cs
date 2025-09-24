using System;
using System.Buffers;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SpectrumAnalyzer.Controls;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly WaterfallControl? _ctrl;
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
        
        _graphUtils.DrawLines(_bitmapDataPool.Span, pointsSpan, Color.FromArgb(255, 255, 128,0));
        
        
        var framePool = ArrayPool<Point>.Shared.Rent(5);
        
        var frameSpan = new Span<Point>(framePool, 0, 5)
        {
            [0] = new Point(0, 0),
            [1] = new Point(_ctrl.WidthPx - 1, 0),
            [2] = new Point(_ctrl.WidthPx - 1, _ctrl.HeightPx - 1),
            [3] = new Point(0, _ctrl.HeightPx - 1),
            [4] = new Point(0, 0),
        };

        _graphUtils.DrawLines(_bitmapDataPool.Span, frameSpan, Color.FromArgb(240, 255, 255, 40));
        
        ArrayPool<Point>.Shared.Return(framePool);
        
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

