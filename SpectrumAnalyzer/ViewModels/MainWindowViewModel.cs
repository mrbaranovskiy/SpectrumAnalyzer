using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IDeviceConnection<ComplexF, UsrpConnectionProperties> _usrpConnection;
    private int _samplingRate;
    private int _bandwidth;
    private int _centerFrequency;
    private IStreamingDataPool<ComplexF> _streamingPool;
    private ITransport<ComplexF> _transport;
    private ComplexDataRenderer _renderer;
    
    private double _minFrequencyAxis;
    private double _maxFrequencyAxis;
    private double _minMagnitudeDbAxis;
    private double _maxMagnitudeDbAxis;

    public List<string> Radios { get; } =
    [
        "Fake Radio",
        "USRP"
    ];

    public int SelectedRadio
    {
        get => _selectedRadio;
        set
        {
            _selectedRadio = value;
            this.RaisePropertyChanged();
        }
    }

    private List<IRendererRepresentation<ComplexF>> _representations;
    
    private FftRepresentation<FFTDrawingProperties> _fftRepresentation;
    private WaterfallRepresentation _waterfallRepresentation;
    
    private FFTDrawingProperties _fftProperties; 
    private WaterfallDrawingProperties _waterfallDrawingProperties;
    
    private double _fftCtrlWidth;
    private double _fftCtrlHeight;

    private double _waterfallCtrlWidth;
    private double _waterfallCtrlHeight;
    private int _selectedRadio;

    public MainWindowViewModel(IDeviceConnection<ComplexF, UsrpConnectionProperties> usrpConnection)
    {
        _usrpConnection = usrpConnection;
        _representations = [];
        //set some defaults
      
        // here is a lot of hardcoded stuff, close to requirements from the task
        // obviously - this is trash code.  
        
        _fftProperties = new FFTDrawingProperties(
            ITransport<ComplexF>.DefaultChunkSize,
            100,
            100,
            Bandwidth,
            CenterFrequency,
            SamplingRate,
            new AxisRange(-80, -20),
            new AxisRange(CenterFrequency,
                CenterFrequency + SamplingRate / 2));

        _waterfallDrawingProperties = new WaterfallDrawingProperties(ITransport<ComplexF>.DefaultChunkSize,
            100, 100,
            Bandwidth,
            CenterFrequency, SamplingRate,
            new AxisRange(-80, -20),
            new AxisRange(CenterFrequency - SamplingRate,
                CenterFrequency + SamplingRate)
        );

        _waterfallRepresentation = new WaterfallRepresentation(_waterfallDrawingProperties);
        _fftRepresentation = new FftRepresentation<FFTDrawingProperties>(_fftProperties);

        MinMagnitudeDbAxis = -100;
        MaxMagnitudeDbAxis = 0;
        Bandwidth = 10_000_000;
        MinFrequencyAxis = 0;
        MaxFrequencyAxis = Bandwidth * 2;
        FftCtrlWidth = 800;
        FftCtrlHeight = 300;
        SamplingRate = Bandwidth / 2;
        CenterFrequency = 98_000_000;
        
        // this is redundant stuff
        MinFrequencyAxis = CenterFrequency;
        MaxFrequencyAxis = CenterFrequency + SamplingRate / 2;
        _representations.Add(_fftRepresentation);
        _representations.Add(_waterfallRepresentation);
    }

    public bool IsStartEnabled => _transport is not { IsStreaming: true };
    public bool IsStopEnabled => !IsStartEnabled;
    
    [RelayCommand(CanExecute = "IsStartEnabled", AllowConcurrentExecutions = true)]
    public Task StartReceiving()
    {
        if(!_representations.Any())
            return Task.CompletedTask;
        
        // it is stupid.... but i have no much time to 
        // implement different devices, device manager and so on..
        var connectionProps = new UsrpConnectionProperties
        {
            Antenna = "RX2",
            BandwidthHz = Bandwidth,
            CenterFrequencyHz = CenterFrequency,
            GainDb = 50,
            SampleRateHz = SamplingRate
        };

        if (SelectedRadio == 0)
        {
            _transport = new FakeTransport(connectionProps, ITransport<ComplexF>.DefaultChunkSize);
        }
        else
        {
            _transport = _usrpConnection.BuildConnection(connectionProps);
            _transport.ReceivingChunkSize = SamplingRate / 25;
            UpdateFftProperties();
            
        }
            
        _streamingPool = new StreamingIQPool(_transport);
        Renderer = new ComplexDataRenderer(_streamingPool);
        
        UpdateFftProperties();

        foreach (var rep in _representations) 
            _renderer.AddRepresentation(rep);

        _ = _transport.Start();
        _streamingPool.DataReceived += HandleDataUpdate;
        StartReceivingCommand.NotifyCanExecuteChanged();
        StopReceivingCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = "IsStopEnabled", AllowConcurrentExecutions = true)]
    public async Task StopReceiving()
    {
        await _transport?.Stop()!;

        // not for production....
        while (_transport is { IsStreaming: true }) 
            await Task.Delay(100);
        
        StartReceivingCommand.NotifyCanExecuteChanged();
        StopReceivingCommand.NotifyCanExecuteChanged();
    }
    public ComplexDataRenderer Renderer
    {
        get => _renderer;
        set
        {
            _renderer = value;
            this.RaisePropertyChanged();
        }
    }

    public int SamplingRate
    {
        get => _samplingRate;
        set
        {
            this.RaiseAndSetIfChanged(ref _samplingRate, value);
            UpdateFftProperties();
        }
    }

    public int Bandwidth
    {
        get => _bandwidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            this.RaiseAndSetIfChanged(ref _bandwidth, value);
            UpdateFftProperties();
        }
    }

    public FftRepresentation<FFTDrawingProperties> FftRepresentation
    {
        get => _fftRepresentation;
        set
        {
            _fftRepresentation = value;
            this.RaisePropertyChanged();
        }
    }

    public WaterfallRepresentation WaterfallRepresentation
    {
        get => _waterfallRepresentation;
        set
        {
            _waterfallRepresentation = value;
            this.RaisePropertyChanged();
        }
    }
    
    public int CenterFrequency
    {
        get => _centerFrequency;
        set
        {
            this.RaiseAndSetIfChanged(ref _centerFrequency, value);
            UpdateFftProperties();
        }
    }
    
    public double WaterfallCtrlWidth
    {
        get => _waterfallCtrlWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _waterfallCtrlWidth, value);
            UpdateFftProperties();
        }
    }
    
    public double WaterfallCtrlHeight
    {
        get => _waterfallCtrlHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _waterfallCtrlHeight, value);
            UpdateFftProperties();
        }
    }

    public double FftCtrlWidth
    {
        get => _fftCtrlWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _fftCtrlWidth, value);
            UpdateFftProperties();
        }
    }
    
    public double FftCtrlHeight
    {
        get => _fftCtrlHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _fftCtrlHeight, value);
            UpdateFftProperties();
        }
    }
    
    public double MinFrequencyAxis
    {
        get => _minFrequencyAxis;
        set
        {
            _minFrequencyAxis = value;
            this.RaiseAndSetIfChanged(ref _minFrequencyAxis, value);
            UpdateFftProperties();
        }
    }

    public double MaxFrequencyAxis
    {
        get => _maxFrequencyAxis;
        set
        {
            _maxFrequencyAxis = value;
            this.RaiseAndSetIfChanged(ref _maxFrequencyAxis, value);
            UpdateFftProperties();
        }
    }

    public double MinMagnitudeDbAxis
    {
        get => _minMagnitudeDbAxis;
        set
        {
            _minMagnitudeDbAxis = value;
            this.RaiseAndSetIfChanged(ref _minMagnitudeDbAxis, value);
            UpdateFftProperties();
        }
    }

    public double MaxMagnitudeDbAxis
    {
        get => _maxMagnitudeDbAxis;
        set
        {
            _maxMagnitudeDbAxis = value;
            this.RaiseAndSetIfChanged(ref _maxMagnitudeDbAxis, value);
            UpdateFftProperties();
        }
    }

    private void UpdateFftProperties()
    {
        if (_fftProperties is null)
            _fftProperties = new FFTDrawingProperties(0,0,0,0,0,0, new AxisRange(0,0), new AxisRange());

        int buffLen = _transport?.ReceivingChunkSize ?? ITransport<ComplexF>.DefaultChunkSize;
        
        _fftProperties = _fftProperties with
        {
            DataBufferLength = buffLen ,
            Width = (int)FftCtrlWidth,
            Height = (int)FftCtrlHeight,
            Bandwidth = Bandwidth,
            CenterFrequency = CenterFrequency,
            SamplingRate = SamplingRate,
            XAxisRange = new AxisRange(_centerFrequency - SamplingRate, _centerFrequency + SamplingRate),
            YAxisRange = new AxisRange(_minMagnitudeDbAxis, _maxMagnitudeDbAxis)
        };
        
        _fftRepresentation.UpdateDrawingProperties(_fftProperties);


        _waterfallDrawingProperties = _waterfallDrawingProperties with
        {
            DataBufferLength = buffLen ,
            Width = (int)WaterfallCtrlWidth,
            Height = (int)WaterfallCtrlHeight,
            Bandwidth = Bandwidth,
            CenterFrequency = CenterFrequency,
            SamplingRate = SamplingRate,
            XAxisRange = new AxisRange(_minFrequencyAxis, _maxFrequencyAxis),
            YAxisRange = new AxisRange(_minMagnitudeDbAxis, _maxMagnitudeDbAxis)
        };
        
        _waterfallRepresentation.UpdateDrawingProperties(_waterfallDrawingProperties);
    }
    
    private void HandleDataUpdate(object? sender, DataReceivedEventArgs e)
    {
        _renderer?.Render();
    }

    public void Dispose(){ }
}
