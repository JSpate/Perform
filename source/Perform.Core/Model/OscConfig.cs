using System.Text.Json.Serialization;

namespace Perform.Model;

public class OscConfig(string ipAddress, int sendPort, int receivePort)
{
    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; } = ipAddress;

    [JsonPropertyName("sendPort")]
    public int SendPort { get; } = sendPort;

    [JsonPropertyName("receivePort")]
    public int ReceivePort { get; } = receivePort;
}