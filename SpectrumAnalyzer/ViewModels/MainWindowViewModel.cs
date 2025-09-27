using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IDeviceConnection<Complex, UsprConnectionProperties> _usrpConnection;
    private int _sampleRate;
    private double _bandwidth;
    private double _centerFrequency;
    private IStreamingDataPool<Complex> _streamingPool;
    public MainWindowViewModel(IDeviceConnection<Complex, UsprConnectionProperties> usrpConnection)
    {
        _usrpConnection = usrpConnection;
        //_streamingPool = new StreamingIQPool();
    }

    [RelayCommand]
    public async Task StartReceiving()
    {
        
    }

    public int SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            this.RaisePropertyChanged();
        }
    }

    public double Bandwidth
    {
        get => _bandwidth;
        set => this.RaiseAndSetIfChanged(ref _bandwidth, value);
    }

    public double CenterFrequency
    {
        get => _centerFrequency;
        set => this.RaiseAndSetIfChanged(ref _centerFrequency, value);
    }

    public void Dispose()
    {
    }
}
