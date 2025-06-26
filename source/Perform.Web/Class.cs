using Microsoft.Extensions.Logging;
using Perform;
using Perform.Model;
using Perform.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Math = Perform.Scripting.MathMethods;

public class GeneratedShowScript
{
    private readonly ShowScript _showScript;
    private readonly ILogger _logger;

    private const int MaxChaseSteps = 4;
    private static bool isSongActive = false;

    public GeneratedShowScript(ShowScript showScript, ILogger logger)
    {
        _showScript = showScript;
        _logger = logger;
    }

    public void Initialise()
    {
        _showScript.AddEventHandler(typeof(EventHandler0001), EventType.StartSong);
        _showScript.AddEventHandler(typeof(EventHandler0002), EventType.EndSong);
        _showScript.AddEventHandler(typeof(EventHandler0003), EventType.BarChange, 4);
        _showScript.AddEventHandler(typeof(EventHandler0004), EventType.BarChange, 8);
        _showScript.AddEventHandler(typeof(EventHandler0005), EventType.ButtonChange, 1, ButtonState.Down);
        _showScript.AddEventHandler(typeof(EventHandler0006), EventType.ButtonChange, 2, ButtonState.Down);
        _showScript.AddEventHandler(typeof(EventHandler0007), EventType.SectionChange, "FXParamOn");
        _showScript.AddEventHandler(typeof(EventHandler0008), EventType.SectionChange, "FXParamOff");
        _showScript.AddEventHandler(typeof(EventHandler0009), EventType.BarChange, 16);
        _showScript.AddEventHandler(typeof(EventHandler0010), EventType.BarChange, 20);
        _showScript.AddEventHandler(typeof(EventHandler0011), EventType.BarChange, 24);
    }

    public void Finalize()
    {
        _showScript.RemoveEventHandler(typeof(EventHandler0001), EventType.StartSong);
        _showScript.RemoveEventHandler(typeof(EventHandler0002), EventType.EndSong);
        _showScript.RemoveEventHandler(typeof(EventHandler0003), EventType.BarChange, 4);
        _showScript.RemoveEventHandler(typeof(EventHandler0004), EventType.BarChange, 8);
        _showScript.RemoveEventHandler(typeof(EventHandler0005), EventType.ButtonChange, 1, ButtonState.Down);
        _showScript.RemoveEventHandler(typeof(EventHandler0006), EventType.ButtonChange, 2, ButtonState.Down);
        _showScript.RemoveEventHandler(typeof(EventHandler0007), EventType.SectionChange, "FXParamOn");
        _showScript.RemoveEventHandler(typeof(EventHandler0008), EventType.SectionChange, "FXParamOff");
        _showScript.RemoveEventHandler(typeof(EventHandler0009), EventType.BarChange, 16);
        _showScript.RemoveEventHandler(typeof(EventHandler0010), EventType.BarChange, 20);
        _showScript.RemoveEventHandler(typeof(EventHandler0011), EventType.BarChange, 24);
    }

    private class EventHandler0001 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0001(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            isSongActive = true;
            if (_showScript.IsAvailable("dmx"))
            {
                _showScript.Set("DMX.MovingHeadLeft.pan", 32768);
                _showScript.Set("DMX.MovingHeadLeft.tilt", 32768);
                _showScript.Set("DMX.MovingHeadLeft.r", 255);
                _showScript.Set("DMX.MovingHeadLeft.g", 0);
                _showScript.Set("DMX.MovingHeadLeft.b", 0);
                _showScript.Set("DMX.MovingHeadLeft.w", 0);
                _showScript.Set("DMX.MovingHeadRight.pan", 32768);
                _showScript.Set("DMX.MovingHeadRight.tilt", 32768);
                _showScript.Set("DMX.MovingHeadRight.r", 255);
                _showScript.Set("DMX.MovingHeadRight.g", 0);
                _showScript.Set("DMX.MovingHeadRight.b", 0);
                _showScript.Set("DMX.MovingHeadRight.w", 0);

                _showScript.Set("DMX.ParLeft.r", 64);
                _showScript.Set("DMX.ParLeft.g", 0);
                _showScript.Set("DMX.ParLeft.b", 0);
                _showScript.Set("DMX.ParRight.r", 64);
                _showScript.Set("DMX.ParRight.g", 0);
                _showScript.Set("DMX.ParRight.b", 0);

            }
            else
            {
                _logger.LogWarning("DMX not available, skipping light setup.");
            }
            if (_showScript.IsAvailable("Reaper"))
            {
                _logger.LogInformation("Reaper is connected.");
            }
            else
            {
                _logger.LogWarning("Reaper is not available.");
            }

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0002 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0002(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            isSongActive = false;
            _showScript.Set("DMX.MovingHeadLeft.r", 0);
            _showScript.Set("DMX.MovingHeadLeft.g", 0);
            _showScript.Set("DMX.MovingHeadLeft.b", 0);
            _showScript.Set("DMX.MovingHeadLeft.w", 0);
            _showScript.Set("DMX.MovingHeadRight.r", 0);
            _showScript.Set("DMX.MovingHeadRight.g", 0);
            _showScript.Set("DMX.MovingHeadRight.b", 0);
            _showScript.Set("DMX.MovingHeadRight.w", 0);
            _showScript.Set("DMX.ParLeft.r", 0);
            _showScript.Set("DMX.ParLeft.g", 0);
            _showScript.Set("DMX.ParLeft.b", 0);
            _showScript.Set("DMX.ParLeft.w", 0);
            _showScript.Set("DMX.ParRight.r", 0);
            _showScript.Set("DMX.ParRight.g", 0);
            _showScript.Set("DMX.ParRight.b", 0);
            _showScript.Set("DMX.ParRight.w", 0);

            _logger.LogInformation("Show ended: all lights off");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0003 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0003(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _showScript.Set("DMX.MovingHeadLeft.r", 0);
            _showScript.Set("DMX.MovingHeadLeft.g", 0);
            _showScript.Set("DMX.MovingHeadLeft.b", 255);
            _showScript.Set("DMX.MovingHeadLeft.w", 0);
            _showScript.Set("DMX.MovingHeadLeft.movementAuto1", 50);
            _showScript.Set("DMX.MovingHeadRight.r", 0);
            _showScript.Set("DMX.MovingHeadRight.g", 0);
            _showScript.Set("DMX.MovingHeadRight.b", 255);
            _showScript.Set("DMX.MovingHeadRight.w", 0);
            _showScript.Set("DMX.MovingHeadRight.movementAuto1", 50);

            _logger.LogInformation("Moving heads set to blue and Auto 1 mode");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0004 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0004(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _showScript.Set("DMX.ParLeft.dimmerStrobeStrobe", 180);
            _showScript.Set("DMX.ParRight.dimmerStrobeStrobe", 180);

            _logger.LogInformation("FrontLights strobe enabled");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0005 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0005(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            var isGreen = _showScript.Get<int>("DMX.MovingHeadLeft.g") > 128;
            if (isGreen)
            {
                _showScript.Set("DMX.MovingHeadLeft.r", 255);
                _showScript.Set("DMX.MovingHeadLeft.g", 0);
                _showScript.Set("DMX.MovingHeadLeft.b", 255);
                _showScript.Set("DMX.MovingHeadLeft.w", 0);
                _showScript.Set("DMX.MovingHeadRight.r", 255);
                _showScript.Set("DMX.MovingHeadRight.g", 0);
                _showScript.Set("DMX.MovingHeadRight.b", 255);
                _showScript.Set("DMX.MovingHeadRight.w", 0);

                _logger.LogInformation("Moving heads set to magenta");
            }
            else
            {
                _showScript.Set("DMX.MovingHeadLeft.r", 0);
                _showScript.Set("DMX.MovingHeadLeft.g", 255);
                _showScript.Set("DMX.MovingHeadLeft.b", 0);
                _showScript.Set("DMX.MovingHeadLeft.w", 0);
                _showScript.Set("DMX.MovingHeadRight.r", 0);
                _showScript.Set("DMX.MovingHeadRight.g", 255);
                _showScript.Set("DMX.MovingHeadRight.b", 0);
                _showScript.Set("DMX.MovingHeadRight.w", 0);

                _logger.LogInformation("Moving heads set to green");
            }

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0006 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.Continue;

        public EventHandler0006(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.LogInformation("Initiating color chase on all RGB channels");

        }

        public void Loop()
        {
            var colors = new[] { (255, 0, 0), (0, 255, 0), (0, 0, 255), (255, 255, 0) };
            var t = _showScript.CurrentSong.Time;
            if (t > 2000)
            {
                _status = ScriptLoopStatus.EndLoop;
                return;
            }
            if (t > 1000)
            {
                return;
            }
            for (var i = 0; i < MaxChaseSteps; i++)
            {
                _showScript.Set("DMX.MovingHeadLeft.r", colors[i].Item1);
                _showScript.Set("DMX.MovingHeadLeft.g", colors[i].Item2);
                _showScript.Set("DMX.MovingHeadLeft.b", colors[i].Item3);
                _showScript.Set("DMX.MovingHeadLeft.w", 0);
                _showScript.Set("DMX.MovingHeadRight.r", colors[i].Item1);
                _showScript.Set("DMX.MovingHeadRight.g", colors[i].Item2);
                _showScript.Set("DMX.MovingHeadRight.b", colors[i].Item3);
                _showScript.Set("DMX.MovingHeadRight.w", 0);

                if (t > 10)
                {
                    continue;
                }
                _logger.LogInformation("Moving heads chase color: " + colors[i].Item1 + "," + colors[i].Item2 + "," + colors[i].Item3);
            }
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0007 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0007(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _showScript.Set("DMX.ParLeft.dimmerStrobeStrobe", 200);
            _showScript.Set("DMX.ParRight.dimmerStrobeStrobe", 200);

            _logger.LogInformation("FrontLights strobe ON (FX param)");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0008 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0008(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _showScript.Set("DMX.ParLeft.dimmerStrobeDimmer", 100);
            _showScript.Set("DMX.ParRight.dimmerStrobeDimmer", 100);

            _logger.LogInformation("FrontLights strobe OFF (FX param)");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0009 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0009(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                _showScript.Set("DMX.MovingHeadLeft.movementSound", 220);
                _showScript.Set("DMX.MovingHeadRight.movementSound", 220);

                _showScript.Set("DMX.ParLeft.r", 0);
                _showScript.Set("DMX.ParLeft.g", 0);
                _showScript.Set("DMX.ParLeft.b", 0);
                _showScript.Set("DMX.ParRight.r", 0);
                _showScript.Set("DMX.ParRight.g", 0);
                _showScript.Set("DMX.ParRight.b", 0);

            }
            catch (Exception e)
            {
                _logger.LogError("Error setting sound mode: " + e.Message);
            }
            finally
            {
                _showScript.Set("DMX.ParLeft.dimmerStrobeDimmer", 0);
                _showScript.Set("DMX.ParRight.dimmerStrobeDimmer", 0);

            }
            _logger.LogInformation("Moving heads in sound mode, FrontLights off");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0010 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public EventHandler0010(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            _showScript.Set("DMX.MovingHeadLeft.reset", 255);
            _showScript.Set("DMX.MovingHeadRight.reset", 255);

            _logger.LogInformation("Moving heads reset to factory defaults");

        }

        public void Loop()
        {

        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0011 : IScriptEventHandler
    {
        private readonly ShowScript _showScript;
        private readonly ILogger _logger;
        private IDeviceScriptFunction _function0001;
        private IDeviceScriptFunction _function0002;
        private ScriptLoopStatus _status = ScriptLoopStatus.Continue;

        public EventHandler0011(ShowScript showScript, ILogger logger)
        {
            _showScript = showScript;
            _logger = logger;
        }

        public void Initialize()
        {
            var x = Math.Sin(7);
            _function0001 = new Perform.Scripting.Functions.Chase([(x, 0, 0), (0, x, 0), (0, 0, x), (x, x, x)], 2);
            _function0001.Initialize(_showScript, ["DMX.MovingHeadLeft", "DMX.MovingHeadRight"]);
            _function0002 = new Perform.Scripting.Functions.Circle(5.5, 15000, 12000, 2);
            _function0002.Initialize(_showScript, ["DMX.MovingHeadLeft", "DMX.MovingHeadRight"]);
        }

        public void Loop()
        {
            if (!_function0001.IsFinished) _function0001.Loop();
            if (!_function0002.IsFinished) _function0002.Loop();
        }


        public ScriptLoopStatus Status => _status;

    }

}
