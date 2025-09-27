using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IDeviceConnection<Complex, UsprConnectionProperties> _usrpConnection;
    private int _sampleRate;
    private int _bandwidth;
    private int _centerFrequency;
    private IStreamingDataPool<Complex> _streamingPool;
    private ITransport<Complex> _transport;
    private ComplexDataRenderer _bitmapRenderer;
    
    private double _minFrequencyAxis;
    private double _maxFrequencyAxis;
    private double _minMagnitudeDbAxis;
    private double _maxMagnitudeDbAxis;

    private List<IRendererRepresentation<Complex>> _representations;
    
    private FftRepresentation _fftRepresentation;
    private WaterfallRepresentation _waterfallRepresentation;
    
    private FFTRepresentationProperties _fftProperties; 
    private WaterfallRepresentationProperties _waterfallRepresentationProperties;
    
    private int _fftCtrlWidth;
    private int _fftCtrlHeight;

    public MainWindowViewModel(IDeviceConnection<Complex, UsprConnectionProperties> usrpConnection)
    {
        _usrpConnection = usrpConnection;
        _representations = new List<IRendererRepresentation<Complex>>();
        _fftProperties = new FFTRepresentationProperties(
            ITransport<Complex>.DefaultChunkSize,
            0,
            0,
            Bandwidth,
            CenterFrequency, SampleRate,
            new AxisRange(-400, 50),
            new AxisRange(0, Bandwidth));
        
        _fftRepresentation = new FftRepresentation(_fftProperties);

        MinMagnitudeDbAxis = -300;
        MaxMagnitudeDbAxis = 60;
        MinFrequencyAxis = 0;
        MaxFrequencyAxis = Bandwidth * 2;
    }

    [RelayCommand]
    public async Task StartReceiving()
    {
        if(!_representations.Any())
            return;
        
        // it is stupid.... but i have no much time to 
        // implement different devices. Device manager and so on..
        var connectionProps = new UsprConnectionProperties
        {
            Antenna = "TX/RX",
            BandwidthHz = Bandwidth,
            CenterFrequencyHz = CenterFrequency,
            GainDb = 50,
            SampleRateHz = SampleRate
        };

        _transport = _usrpConnection.ConnectToDevice(connectionProps);
            
        _streamingPool = new StreamingIQPool(_transport);
        _bitmapRenderer = new ComplexDataRenderer(_streamingPool);
        
        UpdateFftProperties();

        foreach (var rep in _representations) 
            _bitmapRenderer.AddRepresentation(rep);
        
       
        _streamingPool.DataReceived += HandleDataUpdate;
        
    }

    [RelayCommand]
    public async Task StopReceiving() { }

    [RelayCommand]
    public async Task Restart() {}

    public int SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            if (_sampleRate < 1) 
                throw new ArgumentOutOfRangeException(nameof(SampleRate));
            this.RaisePropertyChanged();
            UpdateFftProperties();
        }
    }

    public int Bandwidth
    {
        get => _bandwidth;
        set
        {
            if (_bandwidth < 1) 
                throw new ArgumentOutOfRangeException(nameof(SampleRate));
            this.RaiseAndSetIfChanged(ref _bandwidth, value);
            UpdateFftProperties();
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

    public int FftCtrlWidth
    {
        get => _fftCtrlWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _fftCtrlWidth, value);
            UpdateFftProperties();
        }
    }
    
    public int FftCtrlHeight
    {
        get => _fftCtrlHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _fftCtrlHeight, value);
            UpdateFftProperties();
        }
    }
    
    // Char options

    //

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
            {
                _maxFrequencyAxis = value;
                this.RaiseAndSetIfChanged(ref _maxFrequencyAxis, value);
            }
        }
    }

    public double MinMagnitudeDbAxis
    {
        get => _minMagnitudeDbAxis;
        set
        {
            _minMagnitudeDbAxis = value;
            this.RaiseAndSetIfChanged(ref _minMagnitudeDbAxis, value);
        }
    }

    public double MaxMagnitudeDbAxis
    {
        get => _maxMagnitudeDbAxis;
        set
        {
            _maxMagnitudeDbAxis = value;
            this.RaiseAndSetIfChanged(ref _maxMagnitudeDbAxis, value);
        }
    }

    private void UpdateFftProperties()
    {
        _fftProperties = _fftProperties with
        {
            Width = FftCtrlWidth,
            Height = FftCtrlHeight,
            Bandwidth = Bandwidth,
            CenterFrequency = CenterFrequency,
            SamplingRate = SampleRate,
            XAxisRange = new AxisRange(_minFrequencyAxis, _maxFrequencyAxis),
            YAxisRange = new AxisRange(_minMagnitudeDbAxis, _maxMagnitudeDbAxis)
        };
        
        _fftRepresentation.UpdateDrawingProperties(_fftProperties);
    }

    private void HandleDataUpdate(object? sender, DataReceivedEventArgs e)
    {
        _bitmapRenderer?.Render();
    }

    public void Dispose(){ }
}
