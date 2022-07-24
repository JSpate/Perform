using Perform.OSC.Data;

namespace Perform.OSC;

public interface IClient : IDisposable
{
    void Send(Packet packet);
}