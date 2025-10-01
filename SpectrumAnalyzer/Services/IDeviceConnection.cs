namespace SpectrumAnalyzer.Services;

public interface IDeviceConnection<TData, in TConnectionProps>
    where TConnectionProps : IConnectionProperties
{
    ITransport<TData> BuildConnection(TConnectionProps connectionProps);
}
