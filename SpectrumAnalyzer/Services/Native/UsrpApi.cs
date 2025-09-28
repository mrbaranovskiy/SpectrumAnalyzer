using System;
using System.Text;
using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Services.Native;

public class UsrpApi : IDeviceNativeApi<float>
{
    IntPtr _handle;
    public int Open(string args = "") 
        => UsrpNative.usrp_open(args, out _handle);

    public void Close()
    {
        if(_handle != IntPtr.Zero)
            UsrpNative.usrp_close(_handle);
    }

    public string GetLastError()
    {
        byte[] buffer = new byte[1024];
        UsrpNative.usrp_get_last_error(_handle, buffer, buffer.Length);
        
        return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
    }

    public int ConfigureRx(double frequencyHz, double sampleRata, double gain,
        double bandwidth, string antenna = "TX/RX", string refClk = "internal", int tunning = 0, uint channel = 0)
    {
        if (antenna == null) throw new ArgumentNullException(nameof(antenna));
        if (refClk == null) throw new ArgumentNullException(nameof(refClk));
        ArgumentOutOfRangeException.ThrowIfNegative(tunning);
        ArgumentOutOfRangeException.ThrowIfNegative(frequencyHz);
        ArgumentOutOfRangeException.ThrowIfNegative(sampleRata);
        ArgumentOutOfRangeException.ThrowIfNegative(gain);
        ArgumentOutOfRangeException.ThrowIfNegative(bandwidth);
        
        return UsrpNative.usrp_configure_rx(
            _handle,
            sampleRata,
            frequencyHz,
            gain,
            bandwidth, 
            antenna,
            null!,
            refClk,
            tunning,
            channel);
    }

    public int PrepareStream(uint channel = 0)
    {
        return UsrpNative.usrp_prepare_stream(_handle, "fc32", "fc32", channel);
    }

    public int Receive(float[] buffer, int count ,out int bytesRead)
    {
        bytesRead = 0;
        var err = UsrpNative.usrp_recv_once(_handle, buffer, (UIntPtr)count, 500, out var receivedCount);
        bytesRead = (int)receivedCount;
        return err;
    }

    ~UsrpApi()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        UsrpNative.usrp_close(_handle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}