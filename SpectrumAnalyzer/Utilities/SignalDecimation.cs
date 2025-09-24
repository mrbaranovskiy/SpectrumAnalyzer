using System;
using System.Buffers;

namespace SpectrumAnalyzer.Utilities;

public static class SignalDecimation
{
    //functions does both up- and down- samplings. So we're trying to fit array to the target size.
    // 2n would work the best.
    public static void ResampleData(ReadOnlySpan<double> data, Span<double> output)
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
                output[k] = data[i] * (1.0 - t) + data[i + 1] * t;
            }
        }

        var ps = ArrayPool<double>.Shared.Rent(dataLen + 1);
        for (var i = 0; i < dataLen; i++) ps[i + 1] = ps[i] + data[i];

        var scale = (double)dataLen / outputLen;

        for (var k = 0; k < outputLen; k++)
        {
            var start = (int)Math.Floor(k * scale);
            var endEx = (int)Math.Floor((k + 1) * scale);

            if (endEx <= start) endEx = start + 1;
            if (endEx > dataLen) endEx = dataLen;

            var sum = ps[endEx] - ps[start];
            output[k] = sum / (endEx - start);
        }
        
        ArrayPool<double>.Shared.Return(ps);
    }
}
