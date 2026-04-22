using System;
using System.Net.Sockets;
using System.Text;

public class TcpClientCore
{
    private TcpClient _client = null;
    private NetworkStream _stream = null;

    public bool IsConnected => _client != null && _client.Connected;

    public bool Connect(string ip, int port)
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(ip, port);

            if (_client.Connected)
            {
                _stream = _client.GetStream();
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TcpClientCore] Connect Failed: {e.Message}");
        }

        return false;
    }

    public void Send(byte[] buffer)
    {
        if (!IsConnected)
        {
            Console.WriteLine("[TcpClientCore] Not Connected");
            return;
        }

        if (buffer == null || buffer.Length == 0)
        {
            Console.WriteLine("[TcpClientCore] Invalid buffer");
            return;
        }

        try
        {
            _stream.Write(buffer, 0, buffer.Length);
            Console.WriteLine($"[TcpClientCore] Send {buffer.Length} bytes");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TcpClientCore] Send Failed: {e.Message}");
        }
    }

    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _client?.Close();

            _stream = null;
            _client = null;

            Console.WriteLine("[TcpClientCore] Disconnected");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TcpClientCore] Disconnect Error: {e.Message}");
        }
    }
}