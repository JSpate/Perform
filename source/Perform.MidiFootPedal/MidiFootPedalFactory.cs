using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Perform.Factories;
using Perform.Interfaces;
using Perform.Model;

namespace Perform.MidiFootPedal
{
    public class MidiFootPedalFactory(IMediator mediator, ILogger<MidiController> logger) : IDeviceFactory
    {
        public string Name => "midiFootSwitch";

        public IDevice? CreateDevice(DeviceRecord device)
        {
            var config = JsonSerializer.Deserialize<MidiFootPedalConfig>(JsonSerializer.Serialize(device.Settings));
            if (config == null)
            {
                throw new InvalidOperationException("Invalid config for midi controller");
            }

            return new MidiController(config, mediator, logger);
        }
    }
}
