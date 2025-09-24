using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectrumAnalyzer.Services;

public interface IDataReceived
{
    event EventHandler<EventArgs> DataReceived;
}

//I wish I could use some fancy AsyncEnumerator, but i think, i would be
// diffictul to control the memory pools/ 

public interface ITransport<T> : IDataReceived, IDisposable
{
    /// <summary>
    /// Gets or sets the size of the single received data frame size.
    /// </summary>
    uint InternalPoolChunkSize { get; set; }

    /// <summary>
    /// Gets the current pull chunks number
    /// </summary>
    uint InternalPoolChunkNumber { get; }

    /// <summary>
    /// Gets or sets the max number of pool changes
    /// <remarks>data will be lost in case of overflow</remarks>
    /// </summary>
    uint InternalPoolChunkNumberMax { get; set; }

    bool IsConnected { get; }
    bool IsStreaming { get; }

    DateTime LastDataReceived { get; }
    ReadOnlySpan<T> GetRawData();
}

public class IUsprTransport<TData> : ITransport<TData> where TData : struct
{
    private readonly IDeviceNativeApi<float> _api;

    public IUsprTransport(IDeviceNativeApi<float> api)
    {
        _api = api;
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public event EventHandler<EventArgs>? DataReceived;
    public uint InternalPoolChunkSize { get; set; }
    public uint InternalPoolChunkNumber { get; }
    public uint InternalPoolChunkNumberMax { get; set; }
    public bool IsConnected { get; }
    public bool IsStreaming { get; }
    public DateTime LastDataReceived { get; }

    public ReadOnlySpan<TData> GetRawData()
    {
        throw new NotImplementedException();
    }
}

// var device = Device.UHD.GetDevice();
// ITransport<transport> deviceStream = device.Start(properties); //restart
// 
