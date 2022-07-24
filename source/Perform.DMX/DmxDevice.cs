namespace Perform.DMX
{
    public class DmxDevice
    {
        public DmxDevice(string name, IDictionary<string, DmxAddress> functions)
        {
            Name = name;
            Functions = functions;
        }

        public DmxDevice(string name, DmxDevice dmxType, ushort addressOffset)
        {
            Name = name;
            Functions = new Dictionary<string, DmxAddress>();
            foreach (var dmxTypeFunction in dmxType.Functions)
            {
                Functions.Add(
                    dmxTypeFunction.Key,
                    new DmxAddress(
                        (ushort)(dmxTypeFunction.Value.Address + addressOffset),
                        dmxTypeFunction.Value.Min,
                        dmxTypeFunction.Value.Max,
                        dmxTypeFunction.Value.DefaultValue));
            }
        }

        public string Name { get; }

        public IDictionary<string, DmxAddress> Functions { get; }

        internal void Initialize(DmxCommunicator communicator)
        {
            foreach (var address in Functions.Values)
            {
                address.Initialize(communicator);
            }
        }
    }
}
