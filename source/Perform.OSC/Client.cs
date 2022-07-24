using System.Net.Sockets;
using System.Net;
using Perform.OSC.Data;
using Perform.Model;

namespace Perform.OSC;

internal sealed class Client : IClient
{
    private bool _closing;

    private readonly IPEndPoint _sendIpEndPoint;
    private readonly Socket _sndSocket;

    private readonly UdpClient _receiveUdpClient;
    private IPEndPoint? _receiveIpEndPoint;

    private readonly PacketHandler _oscPacketCallback;
    
    private readonly ManualResetEvent _closingEvent;

    public Client(OscConfig config, PacketHandler callback)
    {
        _oscPacketCallback = callback;

        _closingEvent = new ManualResetEvent(false);

        Exception? lastException = null;

        // try to open the port 10 times, else fail
        for (var i = 0; i < 10; i++)
        {
            try
            {
                _receiveUdpClient = new UdpClient(config.ReceivePort);
                break;
            }
            catch(Exception ex)
            {
                lastException = ex;
                Thread.Sleep(5);
            }
        }

        if (_receiveUdpClient == null)
        {
            throw lastException ?? new Exception($"An unknown error occurred connecting to port {config.ReceivePort}");
        }

        _receiveIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        _sndSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _sendIpEndPoint = new IPEndPoint(IPAddress.Parse(config.IpAddress), config.SendPort);

        // setup first async event
        var callBack = new AsyncCallback(ReceiveCallback);
        _receiveUdpClient.BeginReceive(callBack, null);
    }

    public void Send(Packet packet)
    {
        _sndSocket.SendTo(packet.GetBytes(), _sendIpEndPoint);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        byte[]? bytes = null;

        try
        {
            bytes = _receiveUdpClient.EndReceive(result, ref _receiveIpEndPoint);
        }
        catch (ObjectDisposedException)
        { 
            // Ignore if disposed. This happens when closing the listener
        }

        // Process bytes
        if (bytes is { Length: > 0 })
        {
            Packet? packet = null;
            try
            {
                packet = Packet.GetPacket(bytes);
            }
            catch (Exception)
            {
                // If there is an error reading the packet, null is sent to the callback
            }

            _oscPacketCallback(packet);
        }

        if (_closing)
        {
            _closingEvent.Set();
        }
        else
        {
            var callBack = new AsyncCallback(ReceiveCallback);
            _receiveUdpClient.BeginReceive(callBack, null);
        }
    }

    public void Dispose()
    {
        _closingEvent.Reset();
        _closing = true;
        _receiveUdpClient.Close();
        _closingEvent.WaitOne();

        _sndSocket.Close();
        _sndSocket.Dispose();

        _receiveUdpClient.Dispose();
        _closingEvent.Dispose();
    }
}