using System;
using System.Buffers;
using System.Numerics;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.Utilities;

public static class SignalDecimation
{
    //functions does both up- and down- samplings. So we're trying to fit array to the target size.
    // 2n would work the best.
    public static void ResampleData(ReadOnlySpan<ComplexF> data, Span<ComplexF> output)
    {
        var outputLen = output.Length;
        if (outputLen <= 0) throw new ArgumentOutOfRangeException(nameof(outputLen));
        var dataLen = data.Length;
        
        if (dataLen == 0) return;
        
        if (outputLen >= dataLen)
        {
            if (dataLen == 1)
            {
                for (var k = 0; k < outputLen; k++) output[k] = data[0];
            }
            var r = (double)(dataLen - 1) / (outputLen - 1);
            for (var k = 0; k < outputLen; k++)
            {
                var s = k * r;              
                var i = (int)s;                
                if (i >= dataLen - 1) { output[k] = data[dataLen - 1]; continue; }

                var t = s - i;
                output[k] = data[i].Magnitude * (1.0 - t) + data[i + 1].Magnitude * t;
            }
        }

        var ps = ArrayPool<ComplexF>.Shared.Rent(dataLen + 1);
        for (var i = 0; i < dataLen; i++) ps[i + 1] = ps[i].Magnitude + data[i].Magnitude;

        var scale = (double)dataLen / outputLen;

        for (var k = 0; k < outputLen; k++)
        {
            var start = (int)Math.Floor(k * scale);
            var endEx = (int)Math.Floor((k + 1) * scale);

            if (endEx <= start) endEx = start + 1;
            if (endEx > dataLen) endEx = dataLen;

            var sum = ps[endEx].Magnitude - ps[start].Magnitude;
            output[k] = sum / (endEx - start);
        }
        
        ArrayPool<ComplexF>.Shared.Return(ps);
    }
}
