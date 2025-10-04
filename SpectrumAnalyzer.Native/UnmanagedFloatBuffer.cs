using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Native;

public sealed class UnmanagedFloatBuffer(int length) : IDisposable
{
    public IntPtr Ptr { get; } = Marshal.AllocHGlobal(length * sizeof(float));
    public int Length { get; } = length;
    private bool _disposed;

    public void CopyFrom(float[] src) =>
        Marshal.Copy(src, 0, Ptr, Length);

    public void CopyTo(float[] dst) =>
        Marshal.Copy(Ptr, dst, 0, Length);

    public void Dispose()
    {
        if (_disposed) return;
        Marshal.FreeHGlobal(Ptr);
        _disposed = true;
    }
}