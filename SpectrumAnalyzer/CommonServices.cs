using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using SpectrumAnalyzer.Renderer;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;
using SpectrumAnalyzer.ViewModels;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    { IDeviceConnection<Complex, UsprConnectionProperties> props = new UsprDeviceConnection();
        collection
            .AddSingleton<IDeviceConnection<Complex, UsprConnectionProperties>, UsprDeviceConnection>()
            // .AddSingleton<IDeviceNativeApi<float>, UHDApiFake>()    
            // .AddTransient<ITransport<Complex>, UHDTransport>()
            // .AddSingleton<IStreamingDataPool<Complex>, StreamingIQPool>()
            // .AddSingleton<IBitmapRenderer<Complex>, ComplexDataRenderer>()
            .AddTransient<MainWindowViewModel>();
    }
}
