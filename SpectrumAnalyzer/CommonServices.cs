using Microsoft.Extensions.DependencyInjection;
using SpectrumAnalyzer.ViewModels;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }
}
