using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDeviceConnection : IDeviceConnection<ComplexF, UsrpConnectionProperties>
{
    public ITransport<ComplexF> BuildConnection(UsrpConnectionProperties connectionProps)
    {
        return new UsrpTransport(new UsrpApi(), connectionProps);
    }
}
