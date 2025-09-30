namespace SpectrumAnalyzer.Services;

public interface IConnectionProperties
{
    double CenterFrequencyHz { get; }
    double BandwidthHz { get; }
    double GainDb { get; }
    string Antenna { get; }
    double SampleRateHz { get; }
}