using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.Model;

namespace Perform.Serializers;

public class ChasePropertiesSerializer : JsonConverter<ChaseProperties>
{
    public override ChaseProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var properties = JsonSerializer.Deserialize<IDictionary<string, JsonElement>>(ref reader, options);
        if (properties != null && properties.TryGetValue("trigger", out var triggerEl) && triggerEl.ValueKind == JsonValueKind.String)
        {
            var trigger = triggerEl.GetString();

            if (trigger != null)
            {
                var arrays = new Dictionary<string, int[]>(GetValues(properties));

                return new ChaseProperties(trigger, arrays);
            }
        }

        throw new SerializationException("The chase properties are invalid");

    }

    private IEnumerable<KeyValuePair<string, int[]>> GetValues(IDictionary<string, JsonElement> properties)
    {
        foreach (var (key, el) in properties.Where(k=>k.Key!="trigger"))
        {
            if (el.ValueKind is JsonValueKind.Array)
            {
                var values = el.EnumerateArray()
                    .Select(e => e.TryGetInt32(out var value) ? value : 0)
                    .ToArray();
                yield return new KeyValuePair<string, int[]>(key, values);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ChaseProperties value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}