using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.Config;
using Perform.Model;
using Perform.Model.Console;

namespace Perform.Serializers
{
    internal class ConsoleSerializer : JsonConverter<IConsole>
    {
        public override IConsole? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var consoleName = reader.GetString();

            var intermediate = AsyncHelper.RunSync(async () =>
                await ConfigProvider.Load<IntermediateConsole>($"config/consoles/{consoleName}.json"));

            if (intermediate == null)
            {
                throw new SerializationException("An error occurred deserializing a console");
            }

            var configType = Type.GetType(intermediate.ConfigType);

            if (configType == null)
            {
                throw new SerializationException($"Attempted to deserialize an unknown config type: '{intermediate.ConfigType}'");
            }

            var config = Activator.CreateInstance(configType, intermediate.Config);

            if (config == null)
            {
                throw new SerializationException($"Failed to create an instance of: '{intermediate.ConfigType}'");
            }

            var consoleType = Type.GetType(intermediate.Type);

            if (consoleType == null)
            {
                throw new SerializationException($"Attempted to deserialize an unknown console type: '{intermediate.Type}'");
            }

            if (!consoleType.IsAssignableTo(typeof(IConsole)))
            {
                throw new SerializationException(
                    $"Attempted to deserialize a console type that is not an IConsole: '{intermediate.Type}'");
            }

            if (Activator.CreateInstance(
                    consoleType,
                    intermediate.Name,
                    config
                ) is not IConsole console)
            {
                throw new SerializationException($"Failed to create a console from type: {intermediate.Type}");
            }

            return console;
        }

        public override void Write(Utf8JsonWriter writer, IConsole value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private class IntermediateConsole
        {
            [JsonConstructor]
            public IntermediateConsole(string name, string type, string configType, JsonElement config)
            {
                Name = name;
                Type = type;
                ConfigType = configType;
                Config = config;
            }

            [JsonPropertyName("name")]
            public string Name { get; }

            [JsonPropertyName("type")]
            public string Type { get; }

            [JsonPropertyName("configType")]
            public string ConfigType { get; }

            [JsonPropertyName("config")]
            public JsonElement Config { get; }
        }
    }
}
