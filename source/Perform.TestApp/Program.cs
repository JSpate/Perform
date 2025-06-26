using System.Text.Json;
using Microsoft.Data.Sqlite;
using Perform.Script;
using SQLitePCL;

GrammarTest.RunTest();

Batteries_V2.Init(); // Initialize SQLite native provider

var dbPath = "C:\\dev\\perform\\data\\Perform.db";
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

using var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

// Drop tables if they exist (order matters: child first, then parent)
using (var dropCmd = connection.CreateCommand())
{
    dropCmd.CommandText = @"
        DROP TABLE IF EXISTS DeviceItem;
        DROP TABLE IF EXISTS Device;
        DROP TABLE IF EXISTS Song;
        DROP TABLE IF EXISTS Show;
    ";
    dropCmd.ExecuteNonQuery();
}

// Create tables if they don't exist
var createDeviceTable = @"
CREATE TABLE IF NOT EXISTS Device (
    Id TEXT PRIMARY KEY,
    Type TEXT NOT NULL,
    Description TEXT NOT NULL,
    Settings TEXT NOT NULL
);
";
var createDeviceItemTable = @"
CREATE TABLE IF NOT EXISTS DeviceItem (
    DeviceId TEXT NOT NULL,
    DeviceItem TEXT NOT NULL,
    Config TEXT NOT NULL,
    PRIMARY KEY (DeviceId, DeviceItem),
    FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
";
var createSongTable = @"
CREATE TABLE IF NOT EXISTS Song (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    TimeSignature TEXT,
    Bpm INTEGER,
    Script TEXT,
    Lyrics TEXT
);
";
var createShowTable = @"
CREATE TABLE IF NOT EXISTS Show (
    Id TEXT PRIMARY KEY,
    Data TEXT NOT NULL
);
";

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = createDeviceTable;
    cmd.ExecuteNonQuery();

    cmd.CommandText = createDeviceItemTable;
    cmd.ExecuteNonQuery();

    cmd.CommandText = createSongTable;
    cmd.ExecuteNonQuery();

    cmd.CommandText = createShowTable;
    cmd.ExecuteNonQuery();
}

// Insert DMX device
using (var insertCmd = connection.CreateCommand())
{
    insertCmd.CommandText = @"
        INSERT INTO Device (Id, Type, Description, Settings)
        VALUES ($id, $type, $description, $settings);
    ";
    insertCmd.Parameters.AddWithValue("$id", "Dmx");
    insertCmd.Parameters.AddWithValue("$type", "dmxController");
    insertCmd.Parameters.AddWithValue("$description", "DMX controller for stage lights");
    insertCmd.Parameters.AddWithValue("$settings", @"{
        ""portName"": ""COM3"",
        ""devices"": [
            { ""name"": ""ParLeft"", ""startChannel"": 1, ""type"": ""Par"", ""category"": ""Light"" },
            { ""name"": ""ParRight"", ""startChannel"": 7, ""type"": ""Par"", ""category"": ""Light"" },
            { ""name"": ""MovingHeadLeft"", ""startChannel"": 13, ""type"": ""MovingHead"", ""category"": ""Movement"" },
            { ""name"": ""MovingHeadRight"", ""startChannel"": 27, ""type"": ""MovingHead"", ""category"": ""Movement"" }
        ]
    }");
    insertCmd.ExecuteNonQuery();
}

// Hard-coded JSON for movingHead and par
var movingHeadJson = @"{
    ""pan"": { ""address"": 1, ""range"": [0, 65535], ""description"": ""Pan"", ""category"": ""Movement"" },
    ""tilt"": { ""address"": 3, ""range"": [0, 65535], ""description"": ""Tilt"", ""category"": ""Movement"" },
    ""speed"": { ""address"": 5, ""range"": [0, 255], ""description"": ""Pan / Tilt speed"", ""category"": ""Movement"" },
    ""master"": { ""address"": 6, ""range"": [8, 134], ""description"": ""Dimmer"", ""category"": ""Light"" },
    ""strobe"": { ""address"": 6, ""range"": [135, 239], ""description"": ""Strobe 0Hz to 40Hz"", ""category"": ""Light"" },
    ""open"": { ""address"": 6, ""range"": [240, 255], ""description"": ""Open"", ""category"": ""Light"" },
    ""r"": { ""address"": 7, ""range"": [0, 255], ""description"": ""Red"", ""category"": ""Light"" },
    ""g"": { ""address"": 8, ""range"": [0, 255], ""description"": ""Green"", ""category"": ""Light"" },
    ""b"": { ""address"": 9, ""range"": [0, 255], ""description"": ""Blue"", ""category"": ""Light"" },
    ""w"": { ""address"": 10, ""range"": [0, 255], ""description"": ""White"", ""category"": ""Light"" },
    ""colorMixMixed"": { ""address"": 11, ""range"": [0, 223], ""description"": ""Color Mixed"", ""category"": ""Light"" },
    ""colorMixGradient"": { ""address"": 11, ""range"": [224, 240], ""description"": ""Color gradient from slow to fast"", ""category"": ""Light"" },
    ""colorMixJump"": { ""address"": 11, ""range"": [241, 255], ""description"": ""Color jump from slow to fast"", ""category"": ""Light"" },
    ""colorSpeed"": { ""address"": 12, ""range"": [0, 255], ""description"": ""Color speed"", ""category"": ""Light"" },
    ""movementDMX"": { ""address"": 13, ""range"": [0, 3], ""description"": ""DMX control"", ""category"": ""Movement"" },
    ""movementAuto1"": { ""address"": 13, ""range"": [4, 102], ""description"": ""Auto 1"", ""category"": ""Movement"" },
    ""movementAuto2"": { ""address"": 13, ""range"": [103, 152], ""description"": ""Auto 2"", ""category"": ""Movement"" },
    ""movementAuto3"": { ""address"": 13, ""range"": [153, 203], ""description"": ""Auto 3"", ""category"": ""Movement"" },
    ""movementSound"": { ""address"": 13, ""range"": [204, 255], ""description"": ""Sound"", ""category"": ""Movement"" },
    ""reset"": { ""address"": 14, ""range"": [255], ""description"": ""Factory Reset"", ""category"": ""Movement"" }
}";

var parJson = @"{
    ""master"": { ""address"": 1, ""range"": [0, 255], ""default"": 255, ""category"": ""Light"" },
    ""r"": { ""address"": 2, ""range"": [0, 255], ""default"": 0, ""category"": ""Light"" },
    ""g"": { ""address"": 3, ""range"": [0, 255], ""default"": 0, ""category"": ""Light"" },
    ""b"": { ""address"": 4, ""range"": [0, 255], ""default"": 0, ""category"": ""Light"" },
    ""strobe"": { ""address"": 5, ""range"": [0, 255], ""default"": 0, ""category"": ""Light"" },
    ""gradient"": { ""address"": 6, ""range"": [61, 110], ""category"": ""Light"" },
    ""pulse"": { ""address"": 6, ""range"": [111, 160], ""category"": ""Light"" },
    ""jump"": { ""address"": 6, ""range"": [161, 210], ""category"": ""Light"" },
    ""audio"": { ""address"": 6, ""range"": [211, 255], ""category"": ""Light"" }
}";

// Insert movingHead and par device items for DMX
using (var insertItemCmd = connection.CreateCommand())
{
    insertItemCmd.CommandText = @"
        INSERT INTO DeviceItem (DeviceId, DeviceItem, Config)
        VALUES ($deviceId, $deviceItem, $config);
    ";
    insertItemCmd.Parameters.AddWithValue("$deviceId", "Dmx");
    insertItemCmd.Parameters.AddWithValue("$deviceItem", "MovingHead");
    insertItemCmd.Parameters.AddWithValue("$config", movingHeadJson);
    insertItemCmd.ExecuteNonQuery();

    insertItemCmd.Parameters["$deviceItem"].Value = "Par";
    insertItemCmd.Parameters["$config"].Value = parJson;
    insertItemCmd.ExecuteNonQuery();
}

// Insert Reaper device
using (var insertCmd = connection.CreateCommand())
{
    insertCmd.CommandText = @"
        INSERT INTO Device (Id, Type, Description, Settings)
        VALUES ($id, $type, $description, $settings);
    ";
    insertCmd.Parameters.AddWithValue("$id", "Reaper");
    insertCmd.Parameters.AddWithValue("$type", "reaper");
    insertCmd.Parameters.AddWithValue("$description", "Reaper DAW controller");
    insertCmd.Parameters.AddWithValue("$settings", @"{
        ""ipAddress"": ""127.0.0.1"",
        ""sendPort"": 8000,
        ""receivePort"": 8001,
        ""fileName"": ""./Reaper/Live.RPP"",
        ""tracks"": {
            ""Ibanez"": { ""armed"": false, ""pan"": 0.5 },
            ""Ltd"": { ""armed"": false, ""pan"": 0.5 },
            ""Tele"": { ""armed"": false, ""pan"": 0.5 }
        }
    }");
    insertCmd.ExecuteNonQuery();
}

// Insert FootCtrl (select tch) device
using (var insertCmd = connection.CreateCommand())
{
    insertCmd.CommandText = @"
        INSERT INTO Device (Id, Type, Description, Settings)
        VALUES ($id, $type, $description, $settings);
    ";
    insertCmd.Parameters.AddWithValue("$id", "FootCtrl");
    insertCmd.Parameters.AddWithValue("$type", "midiController");
    insertCmd.Parameters.AddWithValue("$description", "Foot switch MIDI controller");
    insertCmd.Parameters.AddWithValue("$settings", @"{
        ""device"": ""FootCtrl"",
        ""failOverDevice"": ""USB-Midi"",
        ""longPress"": 1000,
        ""controlCodes"": [0, 1, 2, 3]
    }");
    insertCmd.ExecuteNonQuery();
}

// Insert ui32r device
using (var insertCmd = connection.CreateCommand())
{
    insertCmd.CommandText = @"
        INSERT INTO Device (Id, Type, Description, Settings)
        VALUES ($id, $type, $description, $settings);
    ";
    insertCmd.Parameters.AddWithValue("$id", "UI32R");
    insertCmd.Parameters.AddWithValue("$type", "ui32r");
    insertCmd.Parameters.AddWithValue("$description", "UI32R digital mixer");
    insertCmd.Parameters.AddWithValue("$settings", @"{
        ""ipAddress"": ""192.168.1.50""
    }");
    insertCmd.ExecuteNonQuery();
}

// --- Insert Copper Head Road song with lyrics as JSON ---
var songJson = @"
{
  ""name"": ""Copper Head Road"",
  ""description"": ""Ibanez & Ltd - Get the timing right Christy: D G no capo"",
  ""timeSignature"": ""4/4"",
  ""bpm"": 74,
  ""script"": ""redLights.showScript"",
  ""lyrics"": [
    { ""name"": ""Intro"", ""type"": ""intro"", ""lines"": [ { ""bar"": 0, ""text"": ""[Intro]"" } ] },
    { ""name"": ""Verse 1"", ""type"": ""verse"", ""lines"": [
      { ""bar"": 0, ""text"": ""Well my name's John Lee Pettimore [Riff 1] (D)"" },
      { ""bar"": 1, ""text"": ""Same as my daddy and his daddy before [Riff 1] (D)"" },
      { ""bar"": 2, ""text"": ""You hardly ever saw Grandaddy down here [Riff 1] (D)"" },
      { ""bar"": 3, ""text"": ""He only came to town about twice a year [Riff 1] (D)"" },
      { ""bar"": 4, ""text"": ""He'd buy a hundred pounds of yeast and some copper line [Riff 1] (D)"" },
      { ""bar"": 5, ""text"": ""Everybody knew that he made moonshine [Riff 1] (D)"" },
      { ""bar"": 6, ""text"": ""Now the revenue man wanted Grandaddy bad [Riff 1] (G C G)"" },
      { ""bar"": 7, ""text"": ""He headed up the holler with everything he had [Riff 1] (D)"" },
      { ""bar"": 8, ""text"": ""It's before my time but I've been told [Riff 1] (G C G)"" },
      { ""bar"": 9, ""text"": ""He never came back from Copperhead Road [Riff 1] (D)"" }
    ] },
    { ""name"": ""Verse 2"", ""type"": ""verse"", ""lines"": [
      { ""bar"": 0, ""text"": ""Now Daddy ran the whiskey in a big block Dodge [Riff 1] (D)"" },
      { ""bar"": 1, ""text"": ""Bought it at an auction at the Mason's Lodge [Riff 1] (D)"" },
      { ""bar"": 2, ""text"": ""Johnson County Sheriff painted on the side [Riff 1] (D)"" },
      { ""bar"": 3, ""text"": ""Just shot a coat of primer then he looked inside [Riff 1] (D)"" },
      { ""bar"": 4, ""text"": ""Well him and my uncle tore that engine down [Riff 1] (D)"" },
      { ""bar"": 5, ""text"": ""I still remember that rumblin' sound [Riff 1] (D)"" },
      { ""bar"": 6, ""text"": ""Well the sheriff came around in the middle of the night [Riff 1] (G C G)"" },
      { ""bar"": 7, ""text"": ""Heard mama cryin', knew something wasn't right [Riff 1] (D)"" },
      { ""bar"": 8, ""text"": ""He was headed down to Knoxville with the weekly load [Riff 1] (G C G)"" },
      { ""bar"": 9, ""text"": ""You could smell the whiskey burnin' down Copperhead Road [Riff 1] (D let ring)"" }
    ] },
    { ""name"": ""Solo"", ""type"": ""solo"", ""lines"": [
      { ""bar"": 0, ""text"": ""[Solo] (G  C G  D) x2"" }
    ] },
    { ""name"": ""Verse 3"", ""type"": ""verse"", ""lines"": [
      { ""bar"": 0, ""text"": ""I volunteered for the Army on my birthday [Riff 1] (D)"" },
      { ""bar"": 1, ""text"": ""They draft the white trash first, 'round here anyway [Riff 1] (D)"" },
      { ""bar"": 2, ""text"": ""I done two tours of duty in Vietnam [Riff 1] (D)"" },
      { ""bar"": 3, ""text"": ""And I came home with a brand new plan [Riff 1] (D)"" },
      { ""bar"": 4, ""text"": ""I take the seed from Colombia and Mexico [Riff 1] (D)"" },
      { ""bar"": 5, ""text"": ""I plant it up the holler down Copperhead Road [Riff 1] (D)"" },
      { ""bar"": 6, ""text"": ""Well the D.E.A.'s got a chopper in the air [Riff 1] (G C G)"" },
      { ""bar"": 7, ""text"": ""I wake up screaming like I'm back over there [Riff 1] (D)"" },
      { ""bar"": 8, ""text"": ""I learned a thing or two from ol' Charlie don't you know [Riff 1] (G C G)"" },
      { ""bar"": 9, ""text"": ""You better stay away from Copperhead Road [Riff 1] (D)"" }
    ] },
    { ""name"": ""Outro"", ""type"": ""outro"", ""lines"": [
      { ""bar"": 0, ""text"": ""Whoa"" },
      { ""bar"": 1, ""text"": ""Copperhead Road"" },
      { ""bar"": 2, ""text"": ""Copperhead Road"" },
      { ""bar"": 3, ""text"": ""Ha, Copperhead Road"" },
      { ""bar"": 4, ""text"": ""[Riff 1] [Riff 2]"" }
    ] }
  ]
}
";

// Parse JSON and extract fields
using var songDoc = JsonDocument.Parse(songJson);
var songRoot = songDoc.RootElement;

var songId = songRoot.GetProperty("name").GetString()!;
var songName = songId;
var songDescription = songRoot.GetProperty("description").GetString()!;
var timeSignature = songRoot.GetProperty("timeSignature").GetString()!;
var bpm = songRoot.GetProperty("bpm").GetInt32();
var script = songRoot.GetProperty("script").GetString()!;
var lyricsJson = songRoot.GetProperty("lyrics").GetRawText();

// Insert Song (lyrics as JSON)
using (var insertSongCmd = connection.CreateCommand())
{
    insertSongCmd.CommandText = @"
        INSERT INTO Song (Id, Name, Description, TimeSignature, Bpm, Script, Lyrics)
        VALUES ($id, $name, $description, $timeSignature, $bpm, $script, $lyrics);
    ";
    insertSongCmd.Parameters.AddWithValue("$id", songId);
    insertSongCmd.Parameters.AddWithValue("$name", songName);
    insertSongCmd.Parameters.AddWithValue("$description", songDescription);
    insertSongCmd.Parameters.AddWithValue("$timeSignature", timeSignature);
    insertSongCmd.Parameters.AddWithValue("$bpm", bpm);
    insertSongCmd.Parameters.AddWithValue("$script", script);
    insertSongCmd.Parameters.AddWithValue("$lyrics", lyricsJson);
    insertSongCmd.ExecuteNonQuery();
}

// --- Insert Show as atomic JSON ---
var showJson = @"
{
  ""name"": ""Test show"",
  ""notes"": ""This is a test show"",
  ""devices"": [
    {
      ""id"": ""Reaper"",
      ""type"": ""reaper"",
      ""settings"": {
        ""ipAddress"": ""127.0.0.1"",
        ""sendPort"": 8000,
        ""receivePort"": 8001,
        ""fileName"": ""./Reaper/Live.RPP"",
        ""tracks"": {
          ""Ibanez"": { ""armed"": false, ""pan"": 0.5 },
          ""Ltd"": { ""armed"": false, ""pan"": 0.5 },
          ""Tele"": { ""armed"": false, ""pan"": 0.5 },
          ""Wibble"": { ""armed"": false, ""pan"": 0.5 }
        }
      }
    },
    { 
      ""id"": ""UI24R"",
      ""type"": ""ui24r"",
      ""settings"": { ""ipAddress"": ""192.168.1.50"" }
    },
    { 
      ""id"": ""FootSwitch"",
      ""type"": ""midiFootSwitch"",
      ""settings"": {
        ""device"": ""FootCtrl"",
        ""failOverDevice"": ""USB-Midi"",
        ""longPress"": 1000,
        ""controlCodes"": [0, 1, 2, 3]
      }
    },
    { 
      ""id"": ""Dmx"",
      ""type"": ""dmxController"",
      ""settings"": {
        ""portName"": ""COM3"",
        ""devices"": [
          { ""name"": ""parLeft"", ""startChannel"": 1, ""type"": ""par"" },
          { ""name"": ""parRight"", ""startChannel"": 7, ""type"": ""par"" },
          { ""name"": ""movingHeadLeft"", ""startChannel"": 13, ""type"": ""movingHead"" },
          { ""name"": ""movingHeadRight"", ""startChannel"": 27, ""type"": ""movingHead"" }
        ]
      }
    }
  ],
  ""songs"": [
    ""Copper Head Road""
  ]
}
";

using var showDoc = JsonDocument.Parse(showJson);
var showRoot = showDoc.RootElement;

var showId = showRoot.GetProperty("name").GetString()!;

// Insert Show as atomic JSON
using (var insertShowCmd = connection.CreateCommand())
{
    insertShowCmd.CommandText = @"
        INSERT INTO Show (Id, Data)
        VALUES ($id, $data);
    ";
    insertShowCmd.Parameters.AddWithValue("$id", showId);
    insertShowCmd.Parameters.AddWithValue("$data", showJson);
    insertShowCmd.ExecuteNonQuery();
}

Console.WriteLine("Database initialized and atomic show inserted.");


//var show = await ConfigProvider.LoadShow("show.json");
//show.SongChange += (s => Console.WriteLine($"Song is: {s.Name}"));
//show.Start();


//show.Dmx.Devices["SpotLeft"].Functions["R"].Value = 25;
//show.Dmx.Devices["SpotLeft"].Functions["G"].Value = 25;


//await Task.Delay(2000);

//show.Dmx.Devices["SpotLeft"].Functions["G"].Value = 25;
//show.Dmx.Devices["SpotLeft"].Functions["B"].Value = 25;
//show.Dmx.Devices["SpotLeft"].Functions["R"].Value = 0;
//show.Dmx.Devices["SpotLeft"].Functions["W"].Value = 10;

//show.Dmx.Devices["SpotLeft"].Functions["pan"].Value = 65535;
//show.Dmx.Devices["SpotLeft"].Functions["tilt"].Value = 65535;


//var show = await LoadShowAsync("config/shows/show.json");

//IConsole reaper = new Reaper("Reaper", new OscConfig("127.0.0.1", 8000, 8001));

//await Task.Delay(5000);

////// un-mute track 1
//reaper.Tempo = 100;
//if (reaper.TryGetTrack(1, out var track))
//{
//    track.Volume = .5f;
//    track.Armed = true;
//    track.Mute = false;
//    track.Fx(1).Preset = "James";
//    track.Fx(1).Parameter(1).Value = 1;
//}

//static async Task<Show?> LoadShowAsync(string filePath)
//{
//    if (!File.Exists(filePath))
//    {
//        throw new FileNotFoundException(filePath);
//    }

//    try
//    {
//        var options = new JsonSerializerOptions
//        {
//            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//        };
//        await using var openStream = File.OpenRead(filePath);
//        return await JsonSerializer.DeserializeAsync<Show>(openStream, options);
//    }
//    catch (JsonException ex)
//    {
//        Console.WriteLine($"JSON deserialization error: {ex.Message}");
//        return null;
//    }
//}

//Console.ReadLine();
//Console.WriteLine();  
