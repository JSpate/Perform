namespace Perform.Model;

public class Show
{
    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public List<string> Songs { get; set; } = new();

    public List<DeviceRecord> Devices { get; set; } = new();
}