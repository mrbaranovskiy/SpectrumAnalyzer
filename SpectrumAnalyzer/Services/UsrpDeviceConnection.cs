using System;
using System.Numerics;
using System.Threading.Tasks;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDeviceConnection : IDeviceConnection<Complex, UsrpConnectionProperties>
{
    public ITransport<Complex> ConnectToDevice(UsrpConnectionProperties connectionProps)
    {
        return new UsrpTransport(new UsrpApi());
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
