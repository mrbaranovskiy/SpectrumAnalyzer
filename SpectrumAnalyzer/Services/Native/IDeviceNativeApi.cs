namespace SpectrumAnalyzer.Services.Native;

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

public class UHDApiFake : IDeviceNativeApi<float>
{
    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public void SetFrequency(int frequency)
    {
        throw new System.NotImplementedException();
    }

    public void SetBandwidth(int bandwidth)
    {
        throw new System.NotImplementedException();
    }

    public void SetChannels(int channels)
    {
        throw new System.NotImplementedException();
    }

    public void SetRfGain(int rfGain)
    {
        throw new System.NotImplementedException();
    }

    public unsafe float* ReadRawData()
    {
        throw new System.NotImplementedException();
    }
}
