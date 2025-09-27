using System;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Services;

public sealed class DataReceivedEventArgs(int Size, long TimeStamp) : EventArgs
{
    public int Size { get; } = Size;
    public long TimeStamp { get; } = TimeStamp;
}

public interface IDataReceived<T> where T : EventArgs
{
    event EventHandler<DataReceivedEventArgs> DataReceived;
}

//I wish I could use some fancy AsyncEnumerator, but i think, i would be
// diffictul to control the memory pools/ 

public interface ITransport<T> 
    : IDataReceived<DataReceivedEventArgs>, IDisposable
{
    bool IsStreaming { get; }
    DateTime LastDataReceived { get; }
    ReadOnlySpan<T> GetRawData();
    int ReceivingChunkSize { get; set; }
    Task Start();
    Task Stop();
}

// var device = Device.UHD.GetDevice();
// ITransport<transport> deviceStream = device.Start(properties); //restart
// 
