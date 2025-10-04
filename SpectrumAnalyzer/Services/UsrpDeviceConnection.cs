using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Services;

public class UsrpDeviceConnection : IDeviceConnection<ComplexF, SdrConnectionProperties>
{
    public ITransport<ComplexF> BuildConnection(SdrConnectionProperties connectionProps)
    {
        return new UsrpTransport(new UsrpApi(), connectionProps);
    }
}
