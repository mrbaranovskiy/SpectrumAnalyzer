using System;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public interface ISDRDevice<TData, TConnectionProperties> : IDisposable
    where TConnectionProperties : IConnectionProperties
{
    Task<IDeviceConnection<TData, TConnectionProperties>> 
        ConnectAsync(TConnectionProperties connectionProperties);
}