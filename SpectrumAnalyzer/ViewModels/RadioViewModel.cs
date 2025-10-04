using System;
using ReactiveUI;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.ViewModels;

public class RadioViewModel : ViewModelBase, IRadioViewModel
{
    private readonly IDeviceConnection<ComplexF, SdrConnectionProperties> _connectionService;
    private string _header;
    private int _samplingRate;
    private int _bandwidth;
    private int _centerFrequency;
    
    private IStreamingDataPool<ComplexF> _streamingPool;
    private ITransport<ComplexF> _transport;
    private ComplexDataRenderer _renderer;


    public RadioViewModel()
    {
    }
    
    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            this.RaisePropertyChanged();
        }
    }
    
    public int SamplingRate
    {
        get => _samplingRate;
        set
        {
            this.RaiseAndSetIfChanged(ref _samplingRate, value);
        }
    }

    public int Bandwidth
    {
        get => _bandwidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            this.RaiseAndSetIfChanged(ref _bandwidth, value);
        }
    }
    
    public int CenterFrequency
    {
        get => _centerFrequency;
        set
        {
            this.RaiseAndSetIfChanged(ref _centerFrequency, value);
        }
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
}