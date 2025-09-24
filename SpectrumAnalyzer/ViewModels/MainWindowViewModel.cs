using System;
using Avalonia.Threading;

namespace SpectrumAnalyzer.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer _timer;

    public MainWindowViewModel()
    {
        //_timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, HandleDispatcherTimerCallback);
    }

    private static void HandleDispatcherTimerCallback(object sender, EventArgs e)
    {
        
    }
    
    public string Greeting => "Welcome to Avalonia!";

    public void Dispose()
    {
        _timer.Stop();
    }
}
