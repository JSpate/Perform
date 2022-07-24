using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.SequencedEvents;

namespace Perform.Serializers;

public class SequenceSerializer : JsonConverter<Sequence>
{
    public override Sequence Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var values = new List<ISequencedEvent>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var name = reader.GetString();

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
            {
                continue;
            }
            
            var list = new List<object>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        list.Add(reader.GetString() ?? "");
                        break;
                    case JsonTokenType.Number:
                        list.Add(reader.GetInt32());
                        break;
                }
            }

            if (name != null)
            {
                values.Add(SequencedEventFactory.Create(name, list));
            }
        }
            
        return new Sequence(values);
    }

    public override void Write(Utf8JsonWriter writer, Sequence value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}