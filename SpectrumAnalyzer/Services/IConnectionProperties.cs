namespace SpectrumAnalyzer.Services;

public interface IConnectionProperties
{
    double CenterFrequencyHz { get; }
    double BandwidthHz { get; }
    double GainDb { get; }
    string Antenna { get; }
    double SampleRateHz { get; }
}

public record struct UsrpConnectionProperties(
    double CenterFrequencyHz,
    double BandwidthHz,
    double GainDb,
    string Antenna,
    double SampleRateHz)
    : IConnectionProperties;

public static class UsrpConnectionPropertiesExtensions
{
    public static UsrpConnectionProperties GenerateDefault()
    {
        return new UsrpConnectionProperties()
        {
            CenterFrequencyHz = 110_000_000, 
            BandwidthHz = 1_000_000,
            GainDb = 50,
            Antenna = "TX/RX",
            SampleRateHz = 1_000_000
        };
    }
}
