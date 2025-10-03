using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SpectrumAnalyzer.Models;

[StructLayout(LayoutKind.Sequential)]
public struct ComplexF(float imag, float real)
{
    public static implicit operator Complex(ComplexF data) 
        => new(data.Real, data.Imag);

    public static float Abs(ComplexF value) => Hypot(value.Real, value.Imag);

    public float Magnitude => Abs(this);
    
    private static float Hypot(float a, float b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        float d1;
        float d2;
        if (a < b)
        {
            d1 = a;
            d2 = b;
        }
        else
        {
            d1 = b;
            d2 = a;
        }
        if (d1 == 0.0)
            return d2;
        if (float.IsPositiveInfinity(d2) && !float.IsNaN(d1))
            return float.PositiveInfinity;
        float num = d1 / d2;
        return (d2 * MathF.Sqrt((float)(1.0 + num * num)));
    }
    
    public static implicit operator ComplexF(Complex data) 
        => new((float)data.Imaginary, (float)data.Real);

    public static implicit operator ComplexF(float data)
    {
        return new ComplexF(0.0f, data);
    }
    
    public static implicit operator ComplexF(double data)
    {
        return new ComplexF(0.0f, (float)data);
    }

    public float Imag = imag;
    public float Real = real;
}