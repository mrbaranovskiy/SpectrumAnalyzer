using System;
using System.Numerics;

namespace SpectrumAnalyzer.Utilities;
//https://en.wikipedia.org/wiki/In-phase_and_quadrature_components

// i wrote this  before i found ext in FFTSharp :)
public static class SignalGenerator
{
    public static void GenerateRandomIq(
        Span<Complex> output,
        double centerf,
        double sr,
        params (double frequency, double amplitute)[] frequencies)
    {
        // samp_fr == ticks/time => time == ticks/samp_fr 
        var dt = 1/sr;
        double t = 0;
        
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = 0.001 * new Complex( 2 * Math.Cos(2 * Math.PI * centerf * t),0);
            t += dt;
        }
        //
        foreach ((var fr, var amplitude) in frequencies)
        {
            t = 0;
            
            for (var i = 0; i < output.Length; i++)
            {
                // lets generate only Q part.. should be enought for waterfall
                var xt = amplitude * Math.Sin(2 * Math.PI * fr * t);
                output[i] += 0.0001 * new Complex(xt, 0);
        
                t += dt;
            }    
        }
    }
}
