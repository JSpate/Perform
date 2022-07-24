using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.DMX;
using Perform.Model;

namespace Perform.Serializers
{
    internal class DmxAddressSerializer : JsonConverter<DmxAddress>
    {
        public override DmxAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var data = JsonSerializer.Deserialize<JsonElement?>(ref reader, options);

            if (data == null)
            {
                throw new SerializationException("Failed to deserialize the DMX address");
            }

            return ConvertAddress(data.Value);
        }
        
        public DmxAddress ConvertAddress(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Number)
            {
                return new DmxAddress(value.GetByte());
            }

            return new DmxAddress(
                value.GetProperty("address").GetByte(),
                GetUshort(value, "min", 0),
                GetUshort(value, "max", 255),
                GetNullableUshort(value, "default"));
        }

        private static ushort GetUshort(JsonElement el, string name, ushort defaultValue)
        {
            var value = defaultValue;

            if (el.TryGetProperty(name, out var elMin))
            {
                elMin.TryGetUInt16(out value);
            }

            return value;
        }

        private static ushort? GetNullableUshort(JsonElement el, string name)
        {
            ushort? value = null;

            if (el.TryGetProperty(name, out var elMin) && elMin.TryGetUInt16(out var readValue))
            {
                value = readValue;
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DmxAddress value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
