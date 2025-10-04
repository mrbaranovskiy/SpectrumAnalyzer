using Microsoft.Extensions.DependencyInjection;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.ViewModels;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    { IDeviceConnection<ComplexF, SdrConnectionProperties> props = new UsrpDeviceConnection();
        collection
            .AddSingleton<IDeviceConnection<ComplexF, SdrConnectionProperties>, UsrpDeviceConnection>()
            // .AddSingleton<IDeviceNativeApi<float>, UHDApiFake>()    
            // .AddTransient<ITransport<Complex>, UHDTransport>()
            // .AddSingleton<IStreamingDataPool<Complex>, StreamingIQPool>()
            // .AddSingleton<IBitmapRenderer<Complex>, ComplexDataRenderer>()
            .AddTransient<MainWindowViewModel>();
    }
}
