using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Native;

public static class IqFft
{
    public static void Forward(float[] iqInterleaved, float[] outInterleaved, int n)
    {
        if (iqInterleaved.Length != 2 * n || outInterleaved.Length != 2 * n)
            throw new ArgumentException("Buffers must be 2*N floats.");
        var hi = GCHandle.Alloc(iqInterleaved, GCHandleType.Pinned);
        var ho = GCHandle.Alloc(outInterleaved, GCHandleType.Pinned);
        try
        {
            int rc = GpuMath.iq_fft_c2r_forward2(hi.AddrOfPinnedObject(), ho.AddrOfPinnedObject(), n);
            if (rc != 0) throw new InvalidOperationException($"FFT failed: {rc}");
        }
        finally
        {
            if (hi.IsAllocated) hi.Free();
            if (ho.IsAllocated) ho.Free();
        }
    }

    public static void Power(float[] iqInterleaved, float[] powerOut, int n)
    {
        if (iqInterleaved.Length != 2 * n || powerOut.Length != n)
            throw new ArgumentException("Input 2*N, output N.");
        var hi = GCHandle.Alloc(iqInterleaved, GCHandleType.Pinned);
        var ho = GCHandle.Alloc(powerOut, GCHandleType.Pinned);
        try
        {
            int rc = GpuMath.iq_power_spectrum(hi.AddrOfPinnedObject(), ho.AddrOfPinnedObject(), n);
            if (rc != 0) throw new InvalidOperationException($"Power failed: {rc}");
        }
        finally
        {
            if (hi.IsAllocated) hi.Free();
            if (ho.IsAllocated) ho.Free();
        }
    }

    public static void PowerDb(float[] iqInterleaved, float[] dbOut, int n, float floorDb = -160f)
    {
        if (iqInterleaved.Length != 2 * n || dbOut.Length != n)
            throw new ArgumentException("Input 2*N, output N.");
        var hi = GCHandle.Alloc(iqInterleaved, GCHandleType.Pinned);
        var ho = GCHandle.Alloc(dbOut, GCHandleType.Pinned);
        try
        {
            int rc = GpuMath.iq_power_db(hi.AddrOfPinnedObject(), ho.AddrOfPinnedObject(), n, floorDb);
            if (rc != 0) throw new InvalidOperationException($"Power dB failed: {rc}");
        }
        finally
        {
            if (hi.IsAllocated) hi.Free();
            if (ho.IsAllocated) ho.Free();
        }
    }
}