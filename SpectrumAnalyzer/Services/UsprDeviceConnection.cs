using System;
using System.Numerics;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsprDeviceConnection : IDeviceConnection<Complex, UsprConnectionProperties>
{
    public ITransport<Complex> ConnectToDevice(UsprConnectionProperties connectionProps)
    {
        return new UHDTransport(new UHDApiFake());
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
