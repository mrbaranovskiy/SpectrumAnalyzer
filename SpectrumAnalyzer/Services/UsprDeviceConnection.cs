using System;
using System.Numerics;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public class UsprDeviceConnection : IDeviceConnection<Complex, UsprConnectionProperties>
{
    public ITransport<Complex> ConnectToDevice(UsprConnectionProperties connectionProps)
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
