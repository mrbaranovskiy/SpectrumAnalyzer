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

public class UsrpDevice : ISDRDevice<Complex, UsrpConnectionProperties>
{
    private readonly IDeviceNativeApi<float> _api;

    public UsrpDevice(IDeviceNativeApi<float> api)
    {
        _api = api;
    }
    
    public Task<IDeviceConnection<Complex, UsrpConnectionProperties>>
        ConnectAsync(UsrpConnectionProperties connectionProperties)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
} 
