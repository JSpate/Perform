namespace Perform.UI24R;

public class RealTimeData
{
    public RealTimeData(string base64Data)
    {
        var decodedData = Convert.FromBase64String(base64Data);
        var offset = 8;

        offset = ProcessStrips(decodedData, offset, decodedData[0], InputStrips, 6);
        offset = ProcessStrips(decodedData, offset, decodedData[1], MediaStrips, 6);
        offset = ProcessStrips(decodedData, offset, decodedData[2], SubStrips, 7);
        offset = ProcessStrips(decodedData, offset, decodedData[3], FxStrips, 7);
        offset = ProcessStrips(decodedData, offset, decodedData[4], AuxStrips, 5);
        offset = ProcessStrips(decodedData, offset, decodedData[6], MasterStrips, 5);
        ProcessStrips(decodedData, offset, 2, LineInStrips, 6);
    }

    private const double ConversionFactor = 0.004167508166392142;

    public List<ChannelStrip> InputStrips { get; set; } = new();

    public List<ChannelStrip> MediaStrips { get; set; } = new();

    public List<ChannelStrip> SubStrips { get; set; } = new();

    public List<ChannelStrip> FxStrips { get; set; } = new();

    public List<ChannelStrip> AuxStrips { get; set; } = new();

    public List<ChannelStrip> MasterStrips { get; set; } = new();

    public List<ChannelStrip> LineInStrips { get; set; } = new();

    private int ProcessStrips(byte[] decodedData, int offset, int count, List<ChannelStrip> strips, int step)
    {
        for (var i = 0; i < count; i++)
        {
            var strip = new ChannelStrip
            {
                In = ConvertVU(decodedData[offset + 1]),
                Out = ConvertVU(decodedData[offset + 2]),
                Comp = ConvertVU((decodedData[offset + step - 1] & 127) << 1),
                Peak = (decodedData[offset + step - 1] & 128) != 0
            };
            strips.Add(strip);
            offset += step;
        }
        return offset;
    }

    private double ConvertVU(int value)
    {
        var result = ConversionFactor * value;
        return result;
    }
}