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
            0,
            0,
            Bandwidth,
            CenterFrequency, SampleRate,
            new AxisRange(-400, 50),
            new AxisRange(0, Bandwidth));
        
        _fftRepresentation = new FftRepresentation(_fftProperties, 1024);
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
        }
    }

    public int Bandwidth
    {
        get => _bandwidth;
        set => this.RaiseAndSetIfChanged(ref _bandwidth, value);
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

    private double _minFrequency;
    private double _maxFrequency;
    private double _minMagnitudeDb;
    private double _maxMagnitudeDb;

    private void UpdateFftProperties()
    {
        _fftProperties = _fftProperties with
        {
            Width = FftCtrlWidth,
            Height = FftCtrlHeight,
            Bandwidth = Bandwidth,
            CenterFrequency = CenterFrequency,
            SamplingRate = SampleRate,
            // XAxisRange = 
            // YAxisRange =
        };
        
        _fftRepresentation.UpdateDrawingProperties(_fftProperties);
    }

    private void HandleDataUpdate(object? sender, DataReceivedEventArgs e)
    {
        _bitmapRenderer?.Render();
    }

    public void Dispose(){ }
}
