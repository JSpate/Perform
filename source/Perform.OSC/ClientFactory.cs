using Perform.Model;

namespace Perform.OSC;

public class ClientFactory : IClientFactory
{
    public IClient Create(OscConfig config, PacketHandler callback)
    {
        return new Client(config, callback);
    }
}