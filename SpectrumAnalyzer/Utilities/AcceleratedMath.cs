using System;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Native;

namespace SpectrumAnalyzer.Utilities;

public static class AcceleratedMath
{
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

    public static unsafe int FftPower(ReadOnlyMemory<ComplexF> input, Memory<ComplexF> output)
    {
        if (input.Length != output.Length)
            throw new ArgumentException("Input and output must have the same length.");
        
        using var inputMem = input.Pin();
        using var outputMem = output.Pin();
        
        IntPtr inHostPtr = new IntPtr(inputMem.Pointer);
        IntPtr outHostPtr = new IntPtr(outputMem.Pointer);
        
        return GpuMath.iq_power_db(inHostPtr, outHostPtr, input.Length, -160);
    }
}