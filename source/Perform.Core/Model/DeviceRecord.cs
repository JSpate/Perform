using System.Text.Json;

namespace Perform.Model;

public record DeviceRecord(
    string Id,
    string Type,
    string? Description,
    JsonElement Settings
);