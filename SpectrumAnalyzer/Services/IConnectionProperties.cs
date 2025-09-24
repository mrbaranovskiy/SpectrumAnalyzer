namespace SpectrumAnalyzer.Services;

public interface IConnectionProperties
{
    int CenterFrequencyHz { get; }
    int BandwidthHz { get; }
    int GainDb { get; }
    string Antenna { get; }
    int SampleRateHz { get; }
}

public record struct UsprConnectionProperties(
    int CenterFrequencyHz,
    int BandwidthHz,
    int GainDb,
    string Antenna,
    int SampleRateHz)
    : IConnectionProperties;

public static class UsprConnectionPropertiesExtensions
{
    public static UsprConnectionProperties GenerateDefault()
    {
        return new UsprConnectionProperties()
        {
            CenterFrequencyHz = 110_000_000, 
            BandwidthHz = 1_000_000,
            GainDb = 50,
            Antenna = "TX/RX",
            SampleRateHz = 1_000_000
        };
    }
}
