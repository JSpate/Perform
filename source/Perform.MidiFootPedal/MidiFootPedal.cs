using MediatR;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.Extensions.Logging;
using Perform.Model;

namespace Perform.MidiFootPedal;

public class MidiController : IDevice
{
    private readonly IMediator _mediator;
    private readonly ILogger<MidiController> _logger;
    private InputDevice? _inputDevice;
    private readonly List<MidiButton> _midiButtons = [];

    public MidiController(MidiFootPedalConfig config, IMediator mediator, ILogger<MidiController> logger)
    {
        _mediator = mediator;
        _logger = logger;

        if(TryConnect(config, config.Device))
        {
            Start();
            _logger.LogInformation($"Connected to Midi device {config.Device} with {config.ControlCodes.Count} control codes.");
        }
        else
        {
            _inputDevice = null;
            _logger.LogWarning($"Could not connect to Midi device {config.Device}");

            if (!string.IsNullOrEmpty(config.FailOverDevice))
            {
                if (TryConnect(config, config.FailOverDevice))
                {
                    Start();
                    _logger.LogInformation(
                        $"Connected to fail over Midi device {config.FailOverDevice} with {config.ControlCodes.Count} control codes.");
                }
                else
                {
                    _inputDevice = null;
                    _logger.LogWarning(
                        $"Could not connect to fail over Midi device {config.Device}. Midi is not started!");
                }
            }
        }
    }

    private bool TryConnect(MidiFootPedalConfig config, string deviceName)
    {
        try
        {
            _inputDevice = InputDevice.GetByName(deviceName);

            for (var i = 0; i < config.ControlCodes.Count; i++)
            {
                _midiButtons.Add(new MidiButton(config.ControlCodes[i], i, config.LongPress));
            }
            
            _inputDevice.EventReceived += MidiEventReceived;
            _inputDevice.ErrorOccurred += MidiErrorReceived;
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void MidiErrorReceived(object? sender, ErrorOccurredEventArgs e)
    {
        var midiDevice = sender as MidiDevice;
        _logger.LogError($"Error received from '{midiDevice?.Name ?? "Unknown"}' at {DateTime.Now}: {e.Exception}");
    }

    private void MidiEventReceived(object? sender, MidiEventReceivedEventArgs e)
    {
        if (e.Event is ControlChangeEvent change)
        {
            var button = _midiButtons.FirstOrDefault(b => b.ControlCode == change.ControlNumber);

            if (button == null)
            {
                return;
            }

            if (change.ControlValue == 127)
            {
                if (button.Down())
                {
                    _mediator.Publish(new ButtonEvent(button.Index, ButtonState.Down));
                }
            }
            else
            {
                _mediator.Publish(new ButtonEvent(button.Index, ButtonState.Up));
                _mediator.Publish(new ButtonEvent(button.Index, button.LongPress() ? ButtonState.LongPress : ButtonState.Press));
                button.Release();
            }
        }
    }

    public void Start()
    {
        try
        {
            _inputDevice?.StartEventsListening();
        }
        catch(Exception e)
        {
            _logger.LogError($"Error starting listening to MidiFootPedal: {e.Message}", e);
        }
    }
        
    public void Dispose()
    {
        _inputDevice?.Dispose();
    }
}