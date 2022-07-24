using Perform.Model;

namespace Perform.OSC;

public interface IClientFactory
{
    IClient Create(OscConfig config, PacketHandler callback);
}