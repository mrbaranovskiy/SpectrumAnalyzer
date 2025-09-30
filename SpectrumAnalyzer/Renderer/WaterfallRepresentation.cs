using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace SpectrumAnalyzer.Renderer;

public class WaterfallRepresentation : FftRepresentation<WaterfallDrawingProperties>
{
    private byte[] _cyclicScreenBuffer = null;
    private Memory<byte> _cyclicScreenBufferMemory;
    private int _cycleIndex = 0;
    
    public WaterfallRepresentation(WaterfallDrawingProperties properties) : base(properties)
    {
        UpdateDrawingProperties(properties);
    }

    public override void InitBuffers()
    {
        base.InitBuffers();
        
        if (_cyclicScreenBuffer != null)
        {
            BitmapPool.Return(_cyclicScreenBuffer);
            _cycleIndex = 0;
        }
        
        _cyclicScreenBuffer = BitmapPool.Rent(
            DrawingProperties.Width * DrawingProperties.Height * 4);

        _cyclicScreenBufferMemory = new Memory<byte>(_cyclicScreenBuffer);
    }

    protected override void Draw(Memory<Point> generatePoints, Span<double> magnitudes, Span<double> freqs)
    {
        GeneratePoints(generatePoints.Span, magnitudes, freqs);

        var startIndex = _cycleIndex * DrawingProperties.Width * 4;
        var row = _cyclicScreenBufferMemory.Slice(startIndex, DrawingProperties.Width * 4);
        
        for (var i = 1; i < generatePoints.Length; i++)
        {
            var prev =  generatePoints.Span[i - 1];
            var pt =  generatePoints.Span[i];

            var fromIdx = Convert.ToInt32(prev.X) * 4;
            var toIdx = Convert.ToInt32(pt.X) * 4;
            //var color = SampleColor(prev.Y);
            var color = SampleMagnitude(magnitudes[i]);
            
            if (fromIdx == toIdx)
            {
                row.Span[fromIdx + 0] = color.R;
                row.Span[fromIdx + 1] = color.G;
                row.Span[fromIdx + 2] = color.B;
                row.Span[fromIdx + 3] = color.A;
            }
            else
            {
                
                var drawUnit = row.Slice(fromIdx, toIdx - fromIdx);
                for (var j = 0; j < drawUnit.Length; j+=4)
                {
                    drawUnit.Span[j + 0] = color.R;
                    drawUnit.Span[j + 1] = color.G;
                    drawUnit.Span[j + 2] = color.B;
                    drawUnit.Span[j + 3] = color.A;
                }
            }
        }
        
        // copy it to circular buf
        // copy everything to the bitmap.
        
        //todo: better to shift the display buffer 

        var tempCycleIndex = _cycleIndex;
        for (var i = DrawingProperties.Height - 1; i >= 0; i--)
        {
            var idxInCycleBuffer = tempCycleIndex * DrawingProperties.Width * 4;
            var indexInBtmBuffer = i * DrawingProperties.Width * 4;

            var btmSlice = BitmapMemoryHandle.Slice(indexInBtmBuffer, DrawingProperties.Width * 4);
            var crcSlice = _cyclicScreenBufferMemory.Slice(idxInCycleBuffer, DrawingProperties.Width * 4);

            if (btmSlice.Length != crcSlice.Length)
                throw new InvalidOperationException("Something is wrong with buffers strides calculations");
            
            crcSlice.CopyTo(btmSlice);

            tempCycleIndex--;
            
            if (tempCycleIndex < 0)
                tempCycleIndex = DrawingProperties.Height - 1;
        }

        _cycleIndex = ++_cycleIndex % DrawingProperties.Height;
    }


    private List<Color> _colorRange =
    [
        Color.Parse("#0000ff"),
        Color.Parse("#00ffff"),
        Color.Parse("#00ff00"),
        Color.Parse("#ffff00"),
        Color.Parse("#ff0000"),
    ];

    private Color SampleMagnitude(double mag)
    {
        var point = (float)Math.Clamp(Utilities.RangesMapper.Remap(mag,
            DrawingProperties.YAxisRange.Min,
            DrawingProperties.YAxisRange.Max, 0, 1), 0.0, 1.0);

      
        var index = (_colorRange.Count - 1) * point; 
        var low = (int)Math.Floor(index);
        var high = (int)Math.Ceiling(index);

        if (low == high)
            return _colorRange[low];
        
        return Lerp(_colorRange[low], _colorRange[high], point);
    }
    
    
    private Color SampleColor(double h)
    {
        var max = DrawingProperties.Height;
        var level = (float)Math.Clamp( (h) /  max, 0, 1);
        return Lerp(Colors.Yellow, Colors.Black, level);
    }
    
    private static Color Lerp(Color startColor, Color endColor, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);

        var r = (byte)(startColor.R + (endColor.R - startColor.R) * amount);
        var g = (byte)(startColor.G + (endColor.G - startColor.G) * amount);
        var b = (byte)(startColor.B + (endColor.B - startColor.B) * amount);
        var a = (byte)(startColor.A + (endColor.A - startColor.A) * amount);

        return Color.FromArgb(a, r, g, b);
    }

    protected override void HandleDrawingPropertiesUpdated()
    {
        // todo: need to resample the data in the circle buffer.
        // reapply it.
    }
}
