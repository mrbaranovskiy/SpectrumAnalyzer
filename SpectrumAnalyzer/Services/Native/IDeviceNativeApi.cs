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
