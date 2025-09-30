using Avalonia;
using Avalonia.Media;
using SpectrumAnalyzer.Utilities;

namespace SpectrumAnalyzer.Tests;

[TestClass]
public class UtilitiesTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var graphics = BitmapGraphics.CreateGraphics(100, 100, 1.0);
        var buffer = new byte[100 * 100 * 4];
        var image = new Span<byte>(buffer);
        
        Point[]  points = [new(0, 0), new(10, 10), new(100, 100)];
        var pointsSpan = points.AsSpan();
        
        graphics.DrawLines(image, pointsSpan, Color.FromArgb(255, 255, 128, 64));
    }

    [TestMethod]
    public void MapperGivesCorrectValues()
    {
        Assert.AreEqual(RangesMapper.Remap(0.5, 0.0, 1.0, 10, 20), 15);
        Assert.AreEqual(RangesMapper.Remap(0.5, 0.0, 1.0, 0, 100), 50);
        Assert.AreNotEqual(RangesMapper.Remap(0.5, 0.0, 1.0, 0, 200), 101);
    }

    [TestMethod]
    public void ComplexRemapGivesCorrectValues()
    {
        var point = new Point(0.5, 0.5);
        var result = RangesMapper.Map2Point(point, 101, 101, 0.0, 1.0, 0.0, 1.0);
        
        Assert.IsTrue(Math.Abs(result.Item1 - 50) < float.Epsilon);
        Assert.IsTrue(Math.Abs(result.Item2 - 50) < float.Epsilon);
    }
}
