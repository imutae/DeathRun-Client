using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class TcpClientCore
{
    private const int RECEIVE_BUFFER_SIZE = 4096;

    private TcpClient _client = null;
    private NetworkStream _stream = null;

    private readonly PacketParser _packetParser = new PacketParser();
    private readonly byte[] _recvBuffer = new byte[RECEIVE_BUFFER_SIZE];

    private CancellationTokenSource _receiveCts = null;

    private bool _isReceiving = false;
    private bool _isClosed = true;

    private readonly object _stateLock = new object();

    public event Action<Packet> OnPacketReceived;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public bool IsConnected => _client != null && _client.Connected;

    public bool Connect(string ip, int port)
    {
        try
        {
            Disconnect();

            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(ip, port);

            if (_client.Connected)
            {
                _stream = _client.GetStream();
                _packetParser.Clear();

                _receiveCts = new CancellationTokenSource();

                lock (_stateLock)
                {
                    _isClosed = false;
                }

                return true;
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke($"[TcpClientCore] Connect Failed: {e.Message}");
            CloseSocketOnly();
        }

        return false;
    }

    public void StartReceive()
    {
        if (!IsConnected || _stream == null)
        {
            OnError?.Invoke("[TcpClientCore] Cannot start receive. Not connected.");
            return;
        }

        if (_isReceiving)
            return;

        _isReceiving = true;

        _ = ReceiveLoopAsync(_receiveCts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                int bytesRead = await _stream.ReadAsync(_recvBuffer, 0, _recvBuffer.Length);

                if (bytesRead <= 0)
                {
                    break;
                }

                _packetParser.Append(_recvBuffer, bytesRead);

                while (_packetParser.TryGetPacket(out Packet packet))
                {
                    OnPacketReceived?.Invoke(packet);
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            // Disconnect 과정에서 Stream이 닫히면 들어올 수 있음
            if (!token.IsCancellationRequested)
            {
                OnError?.Invoke($"[TcpClientCore] Receive ObjectDisposed: {ex.Message}");
            }
        }
        catch (IOException e)
        {
            if (!token.IsCancellationRequested)
            {
                OnError?.Invoke($"[TcpClientCore] Receive IO Error: {e.Message}");
            }
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
            {
                OnError?.Invoke($"[TcpClientCore] Receive Error: {e.Message}");
            }
        }
        finally
        {
            _isReceiving = false;

            if (!token.IsCancellationRequested)
            {
                Disconnect();
            }
        }
    }

    public void Send(byte[] buffer)
    {
        if (!IsConnected || _stream == null)
        {
            OnError?.Invoke("[TcpClientCore] Not Connected");
            return;
        }

        if (buffer == null || buffer.Length == 0)
        {
            OnError?.Invoke("[TcpClientCore] Invalid buffer");
            return;
        }

        try
        {
            _stream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"[TcpClientCore] Send Failed: {e.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        bool shouldNotify = MarkClosed();

        try
        {
            _receiveCts?.Cancel();

            CloseSocketOnly();
        }
        catch (Exception e)
        {
            OnError?.Invoke($"[TcpClientCore] Disconnect Error: {e.Message}");
        }

        if (shouldNotify)
        {
            OnDisconnected?.Invoke();
        }
    }

    private bool MarkClosed()
    {
        lock (_stateLock)
        {
            if (_isClosed)
                return false;

            _isClosed = true;
            return true;
        }
    }

    private void CloseSocketOnly()
    {
        try
        {
            _stream?.Close();
            _client?.Close();
        }
        finally
        {
            _stream = null;
            _client = null;

            _receiveCts?.Dispose();
            _receiveCts = null;

            _packetParser.Clear();
        }
    }
}