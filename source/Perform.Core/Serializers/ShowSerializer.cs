using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.Config;
using Perform.Model;
using Perform.Model.Console;
using Perform.SequencedEvents;

namespace Perform.Serializers
{
    internal class ShowSerializer : JsonConverter<Show>
    {
        public override Show? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var data = JsonSerializer.Deserialize<IntermediateShow>(ref reader, options);
            if (data == null)
            {
                return null;
            }

            var songs = AsyncHelper.RunSync(() => ConfigProvider.LoadSongs(data.Songs));
            
            return new Show(
                data.LightPositions,
                songs,
                data.Triggers,
                data.Sequences,
                data.Chases,
                data.Consoles,
                data.DmxPort);
        }

        public override void Write(Utf8JsonWriter writer, Show value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }


        private class IntermediateShow
        {
            [JsonConstructor]
            public IntermediateShow(
                IList<string> lightPositions,
                IList<string> songs,
                IDictionary<string, Trigger> triggers,
                IDictionary<string, Sequence> sequences,
                IDictionary<string, IList<ChaseLight>> chases,
                IList<IConsole> consoles,
                string dmxPort)
            {
                LightPositions = lightPositions;
                Triggers = triggers;
                Sequences = sequences;
                Chases = chases;
                Songs = songs;
                Consoles = consoles;
                DmxPort = dmxPort;
            }

            [JsonPropertyName("consoles")]
            public IList<IConsole> Consoles { get; }

            [JsonPropertyName("lightPositions")]
            public IList<string> LightPositions { get; }

            [JsonPropertyName("songs")]
            public IList<string> Songs { get; }

            [JsonPropertyName("triggers")]
            public IDictionary<string, Trigger> Triggers { get; }

            [JsonPropertyName("sequences")]
            public IDictionary<string, Sequence> Sequences { get; }

            [JsonPropertyName("chases")]
            public IDictionary<string, IList<ChaseLight>> Chases { get; }

            [JsonPropertyName("dmxPort")]
            public string DmxPort { get; }
        }
    }
}