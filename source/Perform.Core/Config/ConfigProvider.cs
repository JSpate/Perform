using System.Text.Json;
using System.Text.Json.Serialization;
using Perform.DMX;
using Perform.Model;
using Perform.Serializers;

namespace Perform.Config
{
    public class ConfigProvider
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new ChasePropertiesSerializer(),
                new ConsoleSerializer(),
                new DmxAddressSerializer(),
                new SequenceSerializer(),
                new ShowSerializer()
            }
        };
        
        public static async Task<Show> LoadShow(string file)
        {
            if (file.ToLower().EndsWith(".json"))
            {
                file = file[..^5];
            }

            file = GetPath($"config/shows/{file}.json");
            await using var stream = File.OpenRead(file);
            var show = await JsonSerializer.DeserializeAsync<Show>(stream, Options);

            if (show == null)
            {
                throw new SerializationException($"Could not deserialize show: '{file}'");
            }

            return show;
        }

        internal static string? ConfigPath { get; set; }

        internal static async Task<DmxUniverse> LoadDmxUniverse()
        {
            var deviceTypes = new Dictionary<string, DmxDevice>();
            foreach (var deviceFile in Directory.GetFiles("config/dmxDevices", "*.json"))
            {

                await using var deviceStream = File.OpenRead(deviceFile);
                var device = await JsonSerializer.DeserializeAsync<IDictionary<string, DmxAddress>>(deviceStream, Options);
                if (device != null)
                {
                    var name = Path.GetFileNameWithoutExtension(deviceFile);
                    deviceTypes.Add(name, new DmxDevice(name, device));
                }
            }

            var file = GetPath("config/devices.json");
            await using var stream = File.OpenRead(file);
            var pointers =
                await JsonSerializer.DeserializeAsync<IDictionary<string, DmxDevicePointer>>(stream, Options);

            var dmx = pointers != null
                ? new DmxUniverse(
                    pointers
                        .Select(pointer =>
                            new DmxDevice(
                                pointer.Key,
                                deviceTypes[pointer.Value.Type],
                                pointer.Value.AddressOffset)))
                : null;

            if (dmx==null)
            {
                throw new SerializationException("Failed to deserialize dmx");
            }

            return dmx;
        }

        internal static async Task<Buttons?> LoadButtons()
        {
            var file = GetPath("config/buttons.json");
            await using var stream = File.OpenRead(file);
            return await JsonSerializer.DeserializeAsync<Buttons>(stream, Options);
        }

        internal static async Task<IList<Song>> LoadSongs(IList<string> songs)
        {
            var tasks = songs.Select(LoadSong).ToList();
            await Task.WhenAll(tasks);
            return tasks.Select(t=>t.Result).ToList();
        }

        internal static async Task<Song> LoadSong(string songName)
        {
            var file = GetPath($"config/songs/{songName}.json");
            
            await using var stream = File.OpenRead(file);
            var song = await JsonSerializer.DeserializeAsync<Song>(stream, Options);
         
            if (song == null)
            {
                throw new Exception($"Loading song failed '{file}'");
            }

            return song;
        }

        internal static async Task<T> Load<T>(string filePath)
        {
            var file = GetPath(filePath);
            await using var stream = File.OpenRead(file);
            var data = await JsonSerializer.DeserializeAsync<T>(stream, Options);

            if (data == null)
            {
                throw new Exception($"Loading failed '{file}'");
            }

            return data;
        }

        private static string GetPath(string configFile)
        {
            return ConfigPath == null
                ? Path.Join(ConfigPath, configFile)
                : configFile;
        }

        private class DmxDevicePointer
        {
            [JsonConstructor]
            public DmxDevicePointer(string type, ushort addressOffset)
            {
                Type = type;
                AddressOffset = addressOffset;
            }

            [JsonPropertyName("type")] 
            public string Type { get; }

            [JsonPropertyName("addressOffset")] 
            public ushort AddressOffset { get; }
        }
    }
}