using System;
using System.Numerics;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDeviceConnection : IDeviceConnection<Complex, UsprConnectionProperties>
{
    public ITransport<Complex> ConnectToDevice(UsprConnectionProperties connectionProps)
    {
        return new UsrpTransport(new UsrpApi());
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
