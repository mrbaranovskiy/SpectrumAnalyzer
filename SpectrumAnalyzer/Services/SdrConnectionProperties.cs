namespace SpectrumAnalyzer.Services;

public record struct SdrConnectionProperties(
    double CenterFrequencyHz,
    double BandwidthHz,
    double GainDb,
    string Antenna,
    double SampleRateHz)
    : IConnectionProperties;