using System;
using System.Numerics;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public interface IDeviceConnection<TData, in TConnectionProps> : IDisposable
    where TConnectionProps : IConnectionProperties
{
    ITransport<TData> StartReceiving(TConnectionProps connectionProps);
    ITransport<TData> RestartReceiving(TConnectionProps connectionProps);
    Task<bool> StopReceiving();
}

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

public interface IDeviceNativeApi<TRaw>
{
    void Start();
    void Stop();
    void SetFrequency(int frequency);
    void SetBandwidth(int bandwidth);
    void SetChannels(int channels);
    void  SetRfGain(int rfGain);
    unsafe TRaw* ReadRawData();
}


// public ITransport<Complex> StartReceiving(UsprConnectionProperties connectionProps)
// {
//         
//     return new 
// }
//
// public ITransport<Complex> RestartReceiving(UsprConnectionProperties connectionProps)
// {
//     throw new NotImplementedException();
// }
//
// public Task<bool> StopReceiving()
// {
//     throw new NotImplementedException();
// }
//
//
