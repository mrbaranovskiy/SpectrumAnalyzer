using System.Numerics;
using Moq;
using SpectrumAnalyzer.Services;
using SpectrumAnalyzer.Services.Native;

namespace SpectrumAnalyzer.Tests.Services;

[TestClass]
public class TransportTests
{
    [TestMethod]
    public async Task TestUHDTransportGivesValues()
    {
        // var moq = new Mock<IDeviceNativeApi<float>>();
        // var transport = new UsrpTransport(moq.Object, new UsrpConnectionProperties());
        // transport.Start();
        // Complex[] array = Array.Empty<Complex>();
        // transport.DataReceived += (sender, args) =>
        // {
        //     var data = transport.GetRawData();
        //     array = new Complex[data.Length];
        //     data.CopyTo(array);
        // };
        //
        // await Task.Delay(1000);
        // Assert.IsTrue(array.Any());
    }
}
