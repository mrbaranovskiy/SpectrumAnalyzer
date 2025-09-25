using System;
using System.Numerics;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public class UsprDeviceConnection : IDeviceConnection<Complex, UsprConnectionProperties>
{
    public ITransport<Complex> StartReceiving(UsprConnectionProperties connectionProps)
    {
        throw new NotImplementedException();
    }

    public ITransport<Complex> RestartReceiving(UsprConnectionProperties connectionProps)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StopReceiving()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
