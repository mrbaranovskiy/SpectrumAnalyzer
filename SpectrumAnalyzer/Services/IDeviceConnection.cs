using System;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public interface IDeviceConnection<TData, in TConnectionProps> : IDisposable
    where TConnectionProps : IConnectionProperties
{
    ITransport<TData> ConnectToDevice(TConnectionProps connectionProps);
}
