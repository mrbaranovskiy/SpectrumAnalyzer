namespace SpectrumAnalyzer.Services;

public record struct UsrpConnectionProperties(
    double CenterFrequencyHz,
    double BandwidthHz,
    double GainDb,
    string Antenna,
    double SampleRateHz)
    : IConnectionProperties;