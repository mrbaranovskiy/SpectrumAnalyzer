using System;

namespace SpectrumAnalyzer.Services.Native;

public interface IDeviceNativeApi<TData> : IDisposable
{
    int Open(string args = "");
    void Close();
    string GetLastError();
    int ConfigureRx(double frequencyHz, double sampleRata, double gain,
        double bandwidth, string antenna = "TX/RX", string refClk = "internal", int tunning = 0, uint channel = 0);
    int PrepareStream(uint channel = 0);

    int Receive(TData[] buffer, int count, out int bytesRead);
}