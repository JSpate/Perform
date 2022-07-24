using Perform.Config;

var show = await ConfigProvider.LoadShow("show.json");
show.SongChange += (s => Console.WriteLine($"Song is: {s.Name}"));
show.Start();


show.Dmx.Devices["SpotLeft"].Functions["R"].Value = 25;
show.Dmx.Devices["SpotLeft"].Functions["G"].Value = 25;


await Task.Delay(2000);

show.Dmx.Devices["SpotLeft"].Functions["G"].Value = 25;
show.Dmx.Devices["SpotLeft"].Functions["B"].Value = 25;
show.Dmx.Devices["SpotLeft"].Functions["R"].Value = 0;
show.Dmx.Devices["SpotLeft"].Functions["W"].Value = 10;

show.Dmx.Devices["SpotLeft"].Functions["pan"].Value = 65535;
show.Dmx.Devices["SpotLeft"].Functions["tilt"].Value = 65535;


//// un-mute track 1
//reaper.Track(1).Mute = false;

//// mute track 3
//reaper.Track(3).Mute = true;

//// Set FX preset
//reaper.Track("Steve-Guitar").Fx(1).Preset = "Steve Guitar";

//// Send midi CC
//reaper.SendMidiControlChange(15, 0, 255);

//// Set volume
//reaper.Track("Steve-Guitar2").Volume = .16f;

//reaper.Track(3).Pan = .75f;

//var sequence1 = new Sequence(
//    new MidiNote(0, 40, 128),
//    new Delay(1000),
//    new MidiNote(0,36,192),
//    new Delay(100),
//    new MidiNote(0, 32, 255));

//var sequence2 = new Sequence(
//    new Delay(500),
//    new MidiNote(0, 52, 128),
//    new Delay(1000),
//    new MidiNote(0, 48, 192),
//    new Delay(100),
//    new MidiNote(0, 44, 255));

//await Task.WhenAll(sequence1.Invoke(reaper), sequence2.Invoke(reaper));


Console.WriteLine("Press enter to stop");

Console.ReadLine();