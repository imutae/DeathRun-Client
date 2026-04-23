using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 7777;

    private const string DEFAULT_USER_NAME = "Player1";

    private TcpClientCore tcpClientCore = null;

    private readonly Queue<Packet> receivedPacketQueue = new Queue<Packet>();
    private readonly Queue<string> logQueue = new Queue<string>();

    private readonly object packetQueueLock = new object();
    private readonly object logQueueLock = new object();

    private UnityAction<ChatPacket> onChatReceived;
    public event UnityAction<ChatPacket> OnChatReceived
    {
        add { onChatReceived += value; }
        remove { onChatReceived -= value; }
    }

    protected override void OnInitialize()
    {
        tcpClientCore = new TcpClientCore();

        tcpClientCore.OnPacketReceived += OnPacketReceived;
        tcpClientCore.OnDisconnected += OnDisconnected;
        tcpClientCore.OnError += OnNetworkError;

        bool connected = tcpClientCore.Connect(SERVER_IP, SERVER_PORT);

        if (connected)
        {
            Debug.Log("서버 연결 성공");
            tcpClientCore.StartReceive();
        }
        else
        {
            Debug.LogError("서버 연결 실패");
        }
    }

    private void Update()
    {
        ProcessLogs();
        ProcessReceivedPackets();
    }

    protected override void OnApplicationQuit()
    {
        if (tcpClientCore != null)
        {
            tcpClientCore.OnPacketReceived -= OnPacketReceived;
            tcpClientCore.OnDisconnected -= OnDisconnected;
            tcpClientCore.OnError -= OnNetworkError;

            tcpClientCore.Disconnect();
            tcpClientCore = null;
        }

        base.OnApplicationQuit();
    }

    public void SendChat(string message)
    {
        if (tcpClientCore == null || !tcpClientCore.IsConnected)
        {
            Debug.LogWarning("서버에 연결되지 않음");
            return;
        }

        ChatPacket chatPacket = new ChatPacket
        {
            UserName = DEFAULT_USER_NAME,
            Message = message
        };

        byte[] packet = chatPacket.Serialize();

        tcpClientCore.Send(packet);
    }

    private void OnPacketReceived(Packet packet)
    {
        lock (packetQueueLock)
        {
            receivedPacketQueue.Enqueue(packet);
        }
    }

    private void OnDisconnected()
    {
        EnqueueLog("서버 연결 종료");
    }

    private void OnNetworkError(string message)
    {
        EnqueueLog(message);
    }

    private void ProcessReceivedPackets()
    {
        while (true)
        {
            Packet packet;

            lock (packetQueueLock)
            {
                if (receivedPacketQueue.Count == 0)
                    break;

                packet = receivedPacketQueue.Dequeue();
            }

            HandlePacket(packet);
        }
    }

    private void HandlePacket(Packet packet)
    {
        PacketId packetId = (PacketId)packet.Id;

        switch (packetId)
        {
            case PacketId.S_CHAT:
                HandleChatPacket(packet);
                break;

            case PacketId.E_ACCEPT:
                EAcceptPacket eAcceptPacket = EAcceptPacket.Deserialize(packet.Body);
                PlayerManager.Instance.SetPlayerName(eAcceptPacket.SessionId.ToString());
                break;
            default:
                Debug.LogWarning($"Unknown Packet. Id: {packet.Id}, Size: {packet.Size}");
                break;
        }
    }

    private void HandleChatPacket(Packet packet)
    {
        try
        {
            ChatPacket chatPacket = ChatPacket.Deserialize(packet.Body);
            onChatReceived?.Invoke(chatPacket);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChatPacket 처리 실패: {e.Message}");
        }
    }

    private void EnqueueLog(string message)
    {
        lock (logQueueLock)
        {
            logQueue.Enqueue(message);
        }
    }

    private void ProcessLogs()
    {
        while (true)
        {
            string message;

            lock (logQueueLock)
            {
                if (logQueue.Count == 0)
                    break;

                message = logQueue.Dequeue();
            }

            Debug.Log(message);
        }
    }
}