using System;
using System.Numerics;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public interface ISDRDevice<TData, TConnectionProperties> : IDisposable
    where TConnectionProperties : IConnectionProperties
{
    Task<IDeviceConnection<TData, TConnectionProperties>> 
        ConnectAsync(TConnectionProperties connectionProperties);
}

public class UsprDevice : ISDRDevice<Complex, UsprConnectionProperties>
{
    private readonly IDeviceNativeApi<float> _api;

    public UsprDevice(IDeviceNativeApi<float> api)
    {
        _api = api;
    }
    
    public Task<IDeviceConnection<Complex, UsprConnectionProperties>>
        ConnectAsync(UsprConnectionProperties connectionProperties)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
} 
