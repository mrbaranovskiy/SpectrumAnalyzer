using System;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Utilities;

public static class AcceleratedMath
{
    /// <summary>
    /// FFT forward operation
    /// </summary>
    /// <returns>Return zero if Ok.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static unsafe int FftForward(ReadOnlyMemory<ComplexF> input, Memory<ComplexF> output)
    {
        if (input.Length != output.Length)
            throw new ArgumentException("Input and output must have the same length.");
        
        using var inputMem = input.Pin();
        using var outputMem = output.Pin();
        
        IntPtr inHostPtr = new IntPtr(inputMem.Pointer);
        IntPtr outHostPtr = new IntPtr(outputMem.Pointer);
        
        return GpuMath.iq_fft_c2c_forward(inHostPtr, outHostPtr, input.Length);
    }

    public static unsafe int FftPower(ReadOnlyMemory<ComplexF> input, Memory<float> output)
    {
        if (input.Length != output.Length)
            throw new ArgumentException("Input and output must have the same length.");
        
        using var inputMem = input.Pin();
        using var outputMem = output.Pin();
        
        IntPtr inHostPtr = new IntPtr(inputMem.Pointer);
        IntPtr outHostPtr = new IntPtr(outputMem.Pointer);
        
        return GpuMath.iq_power_db(inHostPtr, outHostPtr, input.Length, -160);
    }
    
    public static void FrequencyScale(Span<float> freqs, int length, float sampleRate, bool positiveOnly = true)
    {
        if (positiveOnly)
        {
            var fftPeriodHz = sampleRate / (length - 1) / 2;
            for (int i = 0; i < length; i++)
                freqs[i] = i * fftPeriodHz;
        }
        else
        {
            var fftPeriodHz = sampleRate / length;
            var halfIndex = length / 2;
            for (int i = 0; i < halfIndex; i++)
                freqs[i] = i * fftPeriodHz;

            for (int i = halfIndex; i < length; i++)
                freqs[i] = -(length - i) * fftPeriodHz;
        }
    }
}