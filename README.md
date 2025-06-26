# Perform
The aim of this project is to allow a band to control thier gig live. 

Perform brings together Mixers, DAW and DMX devices in a show that can be controlled by a band member using a bluetooth foot pedal.

The project aims to be cost effective, the pedal I am using for development cost around £27, the Dmx dongle (RS485) was around £17 and the DMX lights I am using cost around £150 (I had a set of cheap PAR lights and I bought two moving heads second hand).

The DMX communicator started from DMXSimples (https://github.com/BrunoDPO/DMXSimples) 

The OSC code base was based on SharpOSC (https://github.com/ValdemarOrn/SharpOSC)

Thank you to all the open source authors whose code I have drawn in to this.

The initial commit allows control of Reaper (DAW) https://www.reaper.fm/ via Open Sound Control (OSC), and allows control of DMX devices connected via a [serial port](https://amzn.eu/d/6eOI6ZH) (either RS232 or RS485). It also listens to the [bluetooth foot controller](https://amzn.eu/d/gZdbdVU) I purchased (set up as a keyboard as the MIDI model didn't seem to work in Windows 10)


## Next steps:
1. ~~Get the show working~~
2. Implement a basic UI24R client (Okay that wasn't cheap)
3. Add a simple web interface that allows you to:
    - Set default light positions (moving head lights) 
    ~~- View a page that shows the current song, song section and foot pedal actions~~
4. Design a full show and prove the concept
5. Write some tests (bad me, no!)
6. Add a full UI that will allow the creation and management of all the parts that go to make up a show

Using the Reaper model we have now used Perform live to control Guitar sound! (Yay!))

See [here](ShowScript.md) for the scripting language documentation.

### ** Example: show script **
<pre>
bool isSongActive = false;

const MaxChaseSteps = 4;

// At the start of the song, set initial color and movement
on(StartSong) {
    isSongActive = true;
    if (isAvailable(dmx)) {
        set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
            pan = 32768;
            tilt = 32768;
            r = 255;
            g = 0;
            b = 0;
            w = 0;
        }
        set(DMX.ParLeft, DMX.ParRight) {
            r = 64;
            g = 0;
            b = 0;
        }
    } else {
        Log.Warn("DMX not available, skipping light setup.");
    }
    if (isAvailable(Reaper)) {
        Log.Info("Reaper is connected.");
    } else {
        Log.Warn("Reaper is not available.");
    }
}

// At the end of the song, turn off all lights
on(EndSong) {
    isSongActive = false;
    set(DMX.MovingHeadLeft, DMX.MovingHeadRight, DMX.ParLeft, DMX.ParRight) {
        r = 0;
        g = 0;
        b = 0;
        w = 0;
    }
    Log.Info("Show ended: all lights off");
}

// At bar 4, fade moving heads to blue and start auto movement
on(BarChange, 4) {
    set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
        r = 0;
        g = 0;
        b = 255;
        w = 0;
        movementAuto1 = 50;
    }
    Log.Info("Moving heads set to blue and Auto 1 mode");
}

// At bar 8, enable strobe on FrontLights
on(BarChange, 8) {
    set(DMX.ParLeft, DMX.ParRight) {
        dimmerStrobeStrobe = 180;
    }
    Log.Info("FrontLights strobe enabled");
}

// On button1 down, toggle moving head color between green and magenta
on(ButtonChange, 1, ButtonState.Down) {
    var isGreen = get(DMX.MovingHeadLeft.g) > 128;
    if (isGreen) {
        set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
            r = 255;
            g = 0;
            b = 255;
            w = 0;
        }
        Log.Info("Moving heads set to magenta");
    } else {
        set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
            r = 0;
            g = 255;
            b = 0;
            w = 0;
        }
        Log.Info("Moving heads set to green");
    }
}

// On button2 down, run a color chase on all RGB channels
on(ButtonChange, 2, ButtonState.Down) {
    var colors = [ (255,0,0), (0,255,0), (0,0,255), (255,255,0) ];
    while(true) {
        var t = Song.Time();

        if(t>2000){
            break;
        }

        if(t>1000){
            continue;
        }

        for (var i = 0; i < MaxChaseSteps; i++) {
            set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
                r = colors[i][0];
                g = colors[i][1];
                b = colors[i][2];
                w = 0;
            }
            Log.Info("Moving heads chase color: " + colors[i][0] + "," + colors[i][1] + "," + colors[i][2]);
        }
    }
}

// On FX param change, enable or disable strobe on FrontLights
on(SectionChange, "FXParamOn") when(Reaper.fx.param[1] != 0) {
    set(DMX.ParLeft, DMX.ParRight) {
        dimmerStrobeStrobe = 200;
    }
    Log.Info("FrontLights strobe ON (FX param)");
}
on(SectionChange, "FXParamOff") when(Reaper.fx.param[1] == 0) {
    set(DMX.ParLeft, DMX.ParRight) {
        dimmerStrobeDimmer = 100;
    }
    Log.Info("FrontLights strobe OFF (FX param)");
}

// At bar 16, move heads to sound-activated mode and reset FrontLights
on(BarChange, 16) {
    try {
        set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
            movementSound = 220;
        }
        set(DMX.ParLeft, DMX.ParRight) {
            r = 0;
            g = 0;
            b = 0;
        }
    } catch (e) {
        Log.Error("Error setting sound mode: " + e.Message);
    } finally {
        set(DMX.ParLeft, DMX.ParRight) {
            dimmerStrobeDimmer = 0;
        }
    }
    Log.Info("Moving heads in sound mode, FrontLights off");
}

// At bar 20, reset moving heads to factory defaults
on(BarChange, 20) {
    set(DMX.MovingHeadLeft, DMX.MovingHeadRight) {
        reset = 255;
    }
    Log.Info("Moving heads reset to factory defaults");
}

// At bar 24, start circle movement
on(BarChange, 24) {
   var x = Math.Sin(7);
    Lites(DMX.MovingHeadLeft, DMX.MovingHeadRight).Chase([(x, 0, 0), (0, x, 0), (0, 0, x), (x, x, x)]);
    Motion(DMX.MovingHeadLeft, DMX.MovingHeadRight).Circle(5.5, 15000, 12000);
}

</pre>