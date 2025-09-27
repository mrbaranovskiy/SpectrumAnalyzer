using System;
using System.Numerics;

namespace SpectrumAnalyzer.Utilities;
//https://en.wikipedia.org/wiki/In-phase_and_quadrature_components

public static class SignalGenerator
{
    public static void GenerateRandomIQ(
        Span<Complex> output,
        double center_fr,
        double sampling_rate,
        params (double frequency, double amplitute)[] frequencies)
    {
        // samp_fr == ticks/time => time == ticks/samp_fr 
        double dt = 1/sampling_rate;
        double t = 0;
        
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = 0.001 * new Complex( 2 * Math.Cos(2 * Math.PI * center_fr * t),0);
            t += dt;
        }
        //
        foreach ((double fr, double amplitude) in frequencies)
        {
            t = 0;
            
            for (int i = 0; i < output.Length; i++)
            {
                // lets generate only Q part.. should be enought for waterfall
                var xt = amplitude * Math.Sin(2 * Math.PI * fr * t);
                output[i] += 0.000 * new Complex(xt, 0);
        
                t += dt;
            }    
        }
    }
}
