using System.Collections.ObjectModel;

namespace Perform.DMX
{
    public class DmxUniverse
    {
        private DmxCommunicator? _communicator;

        public DmxUniverse(IEnumerable<DmxDevice> devices)
        {
            Devices = new ReadOnlyDictionary<string, DmxDevice>(
                devices.ToDictionary(d => d.Name, d => d));
        }


        public IReadOnlyDictionary<string, DmxDevice> Devices { get; }

        public void Initialize(string portName)
        {
            _communicator = new DmxCommunicator(portName, Devices);
        }

        public List<string> SerialPorts()
        {
            return DmxCommunicator.SerialPorts();
        }

    }
}
