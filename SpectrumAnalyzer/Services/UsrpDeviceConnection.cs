using System.Numerics;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDeviceConnection : IDeviceConnection<Complex, UsrpConnectionProperties>
{
    public ITransport<Complex> BuildConnection(UsrpConnectionProperties connectionProps)
    {
        return new UsrpTransport(new UsrpApi(), connectionProps);
    }
}
