using Microsoft.Extensions.Logging;
using Perform;
using Perform.Model;
using Perform.Interfaces;
using System;
using System.Collections.Generic;
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

    private class EventHandler0001(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            isSongActive = true;
            if (showScript.IsAvailable("dmx") && isSongActive)
            {
                showScript.Set<double>("DMX.MovingHeadLeft.pan", 32768);
                showScript.Set<double>("DMX.MovingHeadLeft.tilt", 32768);
                showScript.Set<double>("DMX.MovingHeadLeft.r", 255);
                showScript.Set<double>("DMX.MovingHeadLeft.g", 0);
                showScript.Set<double>("DMX.MovingHeadLeft.b", 0);
                showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
                showScript.Set<double>("DMX.MovingHeadRight.pan", 32768);
                showScript.Set<double>("DMX.MovingHeadRight.tilt", 32768);
                showScript.Set<double>("DMX.MovingHeadRight.r", 255);
                showScript.Set<double>("DMX.MovingHeadRight.g", 0);
                showScript.Set<double>("DMX.MovingHeadRight.b", 0);
                showScript.Set<double>("DMX.MovingHeadRight.w", 0);

                showScript.Set<double>("DMX.ParLeft.r", 64);
                showScript.Set<double>("DMX.ParLeft.g", 0);
                showScript.Set<double>("DMX.ParLeft.b", 0);
                showScript.Set<double>("DMX.ParRight.r", 64);
                showScript.Set<double>("DMX.ParRight.g", 0);
                showScript.Set<double>("DMX.ParRight.b", 0);

            }
            else
            {
                logger.LogWarning("DMX not available, skipping light setup.");
            }
            if (showScript.IsAvailable("Reaper"))
            {
                logger.LogInformation("Reaper is connected.");
            }
            else
            {
                logger.LogWarning("Reaper is not available.");
            }

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0002(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.Continue;

        public void Initialize()
        {
            isSongActive = false;
            showScript.Set<double>("DMX.MovingHeadLeft.r", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.g", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.b", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
            showScript.Set<double>("DMX.MovingHeadRight.r", 0);
            showScript.Set<double>("DMX.MovingHeadRight.g", 0);
            showScript.Set<double>("DMX.MovingHeadRight.b", 0);
            showScript.Set<double>("DMX.MovingHeadRight.w", 0);
            showScript.Set<double>("DMX.ParLeft.r", 0);
            showScript.Set<double>("DMX.ParLeft.g", 0);
            showScript.Set<double>("DMX.ParLeft.b", 0);
            showScript.Set<double>("DMX.ParLeft.w", 0);
            showScript.Set<double>("DMX.ParRight.r", 0);
            showScript.Set<double>("DMX.ParRight.g", 0);
            showScript.Set<double>("DMX.ParRight.b", 0);
            showScript.Set<double>("DMX.ParRight.w", 0);

            logger.LogInformation("Show ended: all lights off");

        }

        public void Loop()
        {
            showScript.Set<double>("reaper.track1.armed", 1);

            foreach (var x in showScript.Get<IDevice>("reaper"))
            {
                showScript.Set<double>($"reaper.{x}.armed", 1);
                showScript.Set<double>($"reaper.{x}.fx.param[2]", 0);
            }
        }


        public void Finally()
        {
            foreach (var track in showScript.Get<IDevice>("reaper"))
            {
                if (showScript.Get<double>($"reaper.{track}.armed") > 0)
                {
                    showScript.Set<double>($"reaper.{track}.armed", 0);
                    logger.LogInformation("Track " + showScript.Get<double>($"reaper.{track}.name") + " armed state reset.");
                }
            }
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0003(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            showScript.Set<double>("DMX.MovingHeadLeft.r", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.g", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.b", 255);
            showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
            showScript.Set<double>("DMX.MovingHeadLeft.movementAuto1", 50);
            showScript.Set<double>("DMX.MovingHeadRight.r", 0);
            showScript.Set<double>("DMX.MovingHeadRight.g", 0);
            showScript.Set<double>("DMX.MovingHeadRight.b", 255);
            showScript.Set<double>("DMX.MovingHeadRight.w", 0);
            showScript.Set<double>("DMX.MovingHeadRight.movementAuto1", 50);

            logger.LogInformation("Moving heads set to blue and Auto 1 mode");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0004(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            showScript.Set<double>("DMX.ParLeft.dimmerStrobeStrobe", 180);
            showScript.Set<double>("DMX.ParRight.dimmerStrobeStrobe", 180);

            logger.LogInformation("FrontLights strobe enabled");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0005(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            var isGreen = showScript.Get<double>("DMX.MovingHeadLeft.g") > 128;
            if (isGreen)
            {
                showScript.Set<double>("DMX.MovingHeadLeft.r", 255);
                showScript.Set<double>("DMX.MovingHeadLeft.g", 0);
                showScript.Set<double>("DMX.MovingHeadLeft.b", 255);
                showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
                showScript.Set<double>("DMX.MovingHeadRight.r", 255);
                showScript.Set<double>("DMX.MovingHeadRight.g", 0);
                showScript.Set<double>("DMX.MovingHeadRight.b", 255);
                showScript.Set<double>("DMX.MovingHeadRight.w", 0);

                logger.LogInformation("Moving heads set to magenta");
            }
            else
            {
                showScript.Set<double>("DMX.MovingHeadLeft.r", 0);
                showScript.Set<double>("DMX.MovingHeadLeft.g", 255);
                showScript.Set<double>("DMX.MovingHeadLeft.b", 0);
                showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
                showScript.Set<double>("DMX.MovingHeadRight.r", 0);
                showScript.Set<double>("DMX.MovingHeadRight.g", 255);
                showScript.Set<double>("DMX.MovingHeadRight.b", 0);
                showScript.Set<double>("DMX.MovingHeadRight.w", 0);

                logger.LogInformation("Moving heads set to green");
            }

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0006(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.Continue;

        public void Initialize()
        {
            logger.LogInformation("Initiating color chase on all RGB channels");

        }

        public void Loop()
        {
            var colors = new List<int[]> { new[] { 255, 0, 0 }, new[] { 0, 255, 0 }, new[] { 0, 0, 255 }, new[] { 255, 255, 0 } };
            var t = showScript.CurrentSong.Time;
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
                showScript.Set<double>("DMX.MovingHeadLeft.r", colors[i][0]);
                showScript.Set<double>("DMX.MovingHeadLeft.g", colors[i][1]);
                showScript.Set<double>("DMX.MovingHeadLeft.b", colors[i][2]);
                showScript.Set<double>("DMX.MovingHeadLeft.w", 0);
                showScript.Set<double>("DMX.MovingHeadRight.r", colors[i][0]);
                showScript.Set<double>("DMX.MovingHeadRight.g", colors[i][1]);
                showScript.Set<double>("DMX.MovingHeadRight.b", colors[i][2]);
                showScript.Set<double>("DMX.MovingHeadRight.w", 0);

                if (t > 10)
                {
                    continue;
                }
                logger.LogInformation("Moving heads chase color: " + colors[i][0] + "," + colors[i][1] + "," + colors[i][2]);
            }
        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0007(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            showScript.Set<double>("DMX.ParLeft.dimmerStrobeStrobe", 200);
            showScript.Set<double>("DMX.ParRight.dimmerStrobeStrobe", 200);

            logger.LogInformation("FrontLights strobe ON (FX param)");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0008(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            showScript.Set<double>("DMX.ParLeft.dimmerStrobeDimmer", 100);
            showScript.Set<double>("DMX.ParRight.dimmerStrobeDimmer", 100);

            logger.LogInformation("FrontLights strobe OFF (FX param)");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0009(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            try
            {
                showScript.Set<double>("DMX.MovingHeadLeft.movementSound", 220);
                showScript.Set<double>("DMX.MovingHeadRight.movementSound", 220);

                showScript.Set<double>("DMX.ParLeft.r", 0);
                showScript.Set<double>("DMX.ParLeft.g", 0);
                showScript.Set<double>("DMX.ParLeft.b", 0);
                showScript.Set<double>("DMX.ParRight.r", 0);
                showScript.Set<double>("DMX.ParRight.g", 0);
                showScript.Set<double>("DMX.ParRight.b", 0);

            }
            catch (Exception e)
            {
                logger.LogError("Error setting sound mode: " + e.Message);
            }
            finally
            {
                showScript.Set<double>("DMX.ParLeft.dimmerStrobeDimmer", 0);
                showScript.Set<double>("DMX.ParRight.dimmerStrobeDimmer", 0);

            }
            logger.LogInformation("Moving heads in sound mode, FrontLights off");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0010(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {

        private ScriptLoopStatus _status = ScriptLoopStatus.EndLoop;

        public void Initialize()
        {
            showScript.Set<double>("DMX.MovingHeadLeft.reset", 255);
            showScript.Set<double>("DMX.MovingHeadRight.reset", 255);

            logger.LogInformation("Moving heads reset to factory defaults");

        }

        public void Loop()
        {

        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }


    private class EventHandler0011(ShowScript showScript, ILogger logger) : IScriptEventHandler
    {
        private IDeviceScriptFunction? _function0001;
        private IDeviceScriptFunction? _function0002;
        private ScriptLoopStatus _status = ScriptLoopStatus.Continue;

        public void Initialize()
        {
            var x = Math.Sin(7);
            _function0001 = new Perform.Scripting.Functions.Chase([(x, 0, 0), (0, x, 0), (0, 0, x), (x, x, x)], 2);
            _function0001.Initialize(showScript, ["DMX.MovingHeadLeft", "DMX.MovingHeadRight"]);
            _function0002 = new Perform.Scripting.Functions.Circle(5.5, 15000, 12000, 5);
            _function0002.Initialize(showScript, ["DMX.MovingHeadLeft", "DMX.MovingHeadRight"]);
        }

        public void Loop()
        {
            if (_function0001 is { IsFinished: false }) _function0001.Loop();
            if (_function0002 is { IsFinished: false }) _function0002.Loop();
        }


        public void Finally()
        {
            _status = ScriptLoopStatus.Finished;
        }


        public ScriptLoopStatus Status => _status;

    }

}
