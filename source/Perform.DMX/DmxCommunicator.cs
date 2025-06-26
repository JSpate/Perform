using System.IO.Ports;

namespace Perform.DMX;

public class DmxCommunicator
{
    private readonly byte[] _buffer = new byte[513];
    private bool _isActive;
    private Thread? _senderThread;
    private readonly SerialPort _serialPort;
        
    private const int Dmx512BaudRate = 250000;
        
    public DmxCommunicator(string portName, IReadOnlyDictionary<string, DmxDevice> devices)
    {
        _buffer[0] = 0; // The first byte must be a zero
        _serialPort = ConfigureSerialPort(portName);
            
        foreach (var dmxDevice in devices.Values)
        {
            dmxDevice.Initialize(this);
        }

        Start();
    }

    private static SerialPort ConfigureSerialPort(string portName)
    {
        var port = new SerialPort(portName, Dmx512BaudRate, Parity.None, 8, StopBits.Two);

        if (port.IsOpen)
        {
            port.Close();
        }

        // Try to open a connection with the given settings
        port.Open();
        port.Close();

        return port;
    }
        
    public bool IsActive
    {
        get
        {
            lock (this)
            {
                return _isActive;
            }
        }
    }
        
    public byte GetByte(int index)
    {
        if (index < 0 || index > 511)
        {
            throw new IndexOutOfRangeException("Index is not between 0 and 511");
        }

        lock (this)
        {
            return _buffer[index + 1];
        }
    }
        
    public byte[] GetBytes()
    {
        var newBuffer = new byte[512];
        lock (this)
        {
            Array.Copy(_buffer, 1, newBuffer, 0, 512);
            return newBuffer;
        }
    }
        
    public static List<string> SerialPorts()
    {
        var ports = SerialPort.GetPortNames();
        var portNames = new List<string>();
        foreach (var port in ports)
        {
            try
            {
                ConfigureSerialPort(port);
                portNames.Add(port);
            }
            catch
            {
                // This port is not valid
            }
        }
        return portNames;
    }
        
    private void SendBytes()
    {
        while (_isActive)
        {
            // Send a "zero" for 1ms (must send it for at least 100us)
            _serialPort.BreakState = true;
            Thread.Sleep(1);
            _serialPort.BreakState = false;
            // Send all the byte parameters
            _serialPort.Write(_buffer, 0, _buffer.Length);
        }
    }
        
    public void SetByte(int index, byte value)
    {
        if (index < 0 || index > 511)
            throw new IndexOutOfRangeException("Index is not between 0 and 511");

        lock (this)
        {
            _buffer[index + 1] = value;
        }
    }
        
    public void SetBytes(byte[] newBuffer)
    {
        if (newBuffer.Length != 512)
            throw new ArgumentException("This byte array does not contain 512 elements", "newBuffer");

        lock (this)
        {
            Array.Copy(newBuffer, 0, _buffer, 1, 512);
        }
    }

    public void SetBytes(int index, byte[] bytes)
    {
        if (index < 0 || index > 511-bytes.Length)
        {
            throw new IndexOutOfRangeException("Index is not between 0 and 511");
        }

        lock (this)
        {
            bytes.CopyTo(_buffer, index + 1);
        }
    }

    public void Start()
    {
        if (!IsActive)
        {
            lock (this)
            {
                if (!IsActive)
                {
                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.Open();
                        _isActive = true;
                    }

                    if (_serialPort.IsOpen)
                    {
                        _senderThread = new Thread(SendBytes);
                        _senderThread.Start();
                    }
                }
            }
        }
    }
        
    public void Stop()
    {
        if (!IsActive)
        {
            return;
        }

        lock (this)
        {
            if (!IsActive)
            {
                return;
            }

            try
            {
                _senderThread?.Join(1000);
            }
            catch (Exception)
            {
                // TODO: Better exception handling
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _isActive = false;
        }
    }
}