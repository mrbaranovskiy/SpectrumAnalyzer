using System;
using System.Numerics;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDevice : ISDRDevice<Complex, SdrConnectionProperties>
{
    private readonly IDeviceNativeApi<float> _api;

    public UsrpDevice(IDeviceNativeApi<float> api)
    {
        _api = api;
    }
    
    public Task<IDeviceConnection<Complex, SdrConnectionProperties>>
        ConnectAsync(SdrConnectionProperties connectionProperties)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}