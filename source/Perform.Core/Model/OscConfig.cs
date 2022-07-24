using System.Text.Json;
using System.Text.Json.Serialization;

namespace Perform.Model;

public class OscConfig
{
    public OscConfig(string ipAddress, int sendPort, int receivePort)
    {
        IpAddress = ipAddress;
        SendPort = sendPort;
        ReceivePort = receivePort;
    }

    public OscConfig(JsonElement element)
    {
        IpAddress = element.GetProperty("ipAddress").GetString() ?? "";
        SendPort = element.GetProperty("sendPort").GetInt32();
        ReceivePort = element.GetProperty("receivePort").GetInt32();
    }

    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; }

    [JsonPropertyName("sendPort")]
    public int SendPort { get; }

    [JsonPropertyName("receivePort")]
    public int ReceivePort { get; }
}