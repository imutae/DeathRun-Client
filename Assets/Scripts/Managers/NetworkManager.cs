using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 7777;

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

    private UnityAction<SRoomListPacket> onRoomListReceived;
    public event UnityAction<SRoomListPacket> OnRoomListReceived
    {
        add { onRoomListReceived += value; }
        remove { onRoomListReceived -= value; }
    }

    private UnityAction<SJoinPacket> onJoinResultReceived;
    public event UnityAction<SJoinPacket> OnJoinResultReceived
    {
        add { onJoinResultReceived += value; }
        remove { onJoinResultReceived -= value; }
    }

    private UnityAction<long> onOtherPlayerJoined;
    public event UnityAction<long> OnOtherPlayerJoined
    {
        add { onOtherPlayerJoined += value; }
        remove { onOtherPlayerJoined -= value; }
    }

    private UnityAction<long> onOtherPlayerLeft;
    public event UnityAction<long> OnOtherPlayerLeft
    {
        add { onOtherPlayerLeft += value; }
        remove { onOtherPlayerLeft -= value; }
    }

    private UnityAction<SMovePacket> onMoveReceived;
    public event UnityAction<SMovePacket> OnMoveReceived
    {
        add { onMoveReceived += value; }
        remove { onMoveReceived -= value; }
    }

    private UnityAction onLeftRoom;
    public event UnityAction OnLeftRoom
    {
        add { onLeftRoom += value; }
        remove { onLeftRoom -= value; }
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

    private bool CanSend()
    {
        if (tcpClientCore == null || !tcpClientCore.IsConnected)
        {
            Debug.LogWarning("서버에 연결되지 않음");
            return false;
        }

        return true;
    }

    public void SendChat(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (!CanSend())
            return;

        ChatPacket chatPacket = new ChatPacket
        {
            UserName = PlayerManager.Instance.PlayerName,
            Message = message
        };

        tcpClientCore.Send(chatPacket.Serialize());
    }

    public void RequestRoomList()
    {
        if (!CanSend())
            return;

        RRoomListPacket packet = new RRoomListPacket();
        tcpClientCore.Send(packet.Serialize());
    }

    public void CreateRoom()
    {
        JoinRoom(ProtocolConstants.InvalidRoomId);
    }

    public void JoinRoom(ushort roomId)
    {
        if (!CanSend())
            return;

        RJoinPacket packet = new RJoinPacket
        {
            RoomId = roomId
        };

        tcpClientCore.Send(packet.Serialize());
    }

    public void SendMove(Vector2 position)
    {
        if (!CanSend())
            return;

        RMovePacket packet = new RMovePacket
        {
            X = position.x,
            Y = position.y
        };

        tcpClientCore.Send(packet.Serialize());
    }

    public void LeaveRoom()
    {
        if (!CanSend())
            return;

        NLeavePacket packet = new NLeavePacket();
        tcpClientCore.Send(packet.Serialize());

        // N_LEAVE는 클라 -> 서버 일방 알림이므로 서버 응답을 기다리지 않는다.
        onLeftRoom?.Invoke();
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
                HandleAcceptPacket(packet);
                break;

            case PacketId.S_JOIN:
                HandleJoinPacket(packet);
                break;

            case PacketId.S_MOVE:
                HandleMovePacket(packet);
                break;

            case PacketId.E_JOIN:
                HandleOtherPlayerJoinedPacket(packet);
                break;

            case PacketId.E_LEAVE:
                HandleOtherPlayerLeftPacket(packet);
                break;

            case PacketId.S_ROOM_LIST:
                HandleRoomListPacket(packet);
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
        catch (Exception e)
        {
            Debug.LogError($"ChatPacket 처리 실패: {e.Message}");
        }
    }

    private void HandleAcceptPacket(Packet packet)
    {
        try
        {
            EAcceptPacket eAcceptPacket = EAcceptPacket.Deserialize(packet.Body);
            PlayerManager.Instance.SetPlayerName(eAcceptPacket.SessionId);

            Debug.Log($"SessionId 수신: {eAcceptPacket.SessionId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"EAcceptPacket 처리 실패: {e.Message}");
        }
    }

    private void HandleJoinPacket(Packet packet)
    {
        try
        {
            SJoinPacket joinPacket = SJoinPacket.Deserialize(packet.Body);

            if (!joinPacket.IsSuccess)
            {
                Debug.LogWarning("방 생성/참가 실패");
            }
            else
            {
                Debug.Log($"방 생성/참가 성공. PlayerCount: {joinPacket.PlayerCount}");
            }

            onJoinResultReceived?.Invoke(joinPacket);
        }
        catch (Exception e)
        {
            Debug.LogError($"SJoinPacket 처리 실패: {e.Message}");
        }
    }

    private void HandleMovePacket(Packet packet)
    {
        try
        {
            SMovePacket movePacket = SMovePacket.Deserialize(packet.Body);

            // 서버가 내 이동도 다시 브로드캐스트하므로,
            // 실제 반영하는 쪽에서 내 sessionId면 무시해도 된다.
            onMoveReceived?.Invoke(movePacket);
        }
        catch (Exception e)
        {
            Debug.LogError($"SMovePacket 처리 실패: {e.Message}");
        }
    }

    private void HandleOtherPlayerJoinedPacket(Packet packet)
    {
        try
        {
            EJoinPacket joinPacket = EJoinPacket.Deserialize(packet.Body);

            Debug.Log($"다른 플레이어 입장: {joinPacket.SessionId}");
            onOtherPlayerJoined?.Invoke(joinPacket.SessionId);
        }
        catch (Exception e)
        {
            Debug.LogError($"EJoinPacket 처리 실패: {e.Message}");
        }
    }

    private void HandleOtherPlayerLeftPacket(Packet packet)
    {
        try
        {
            ELeavePacket leavePacket = ELeavePacket.Deserialize(packet.Body);

            Debug.Log($"다른 플레이어 퇴장: {leavePacket.SessionId}");
            onOtherPlayerLeft?.Invoke(leavePacket.SessionId);
        }
        catch (Exception e)
        {
            Debug.LogError($"ELeavePacket 처리 실패: {e.Message}");
        }
    }

    private void HandleRoomListPacket(Packet packet)
    {
        try
        {
            SRoomListPacket roomListPacket = SRoomListPacket.Deserialize(packet.Body);

            Debug.Log($"방 목록 수신. RoomCount: {roomListPacket.RoomCount}");
            onRoomListReceived?.Invoke(roomListPacket);
        }
        catch (Exception e)
        {
            Debug.LogError($"SRoomListPacket 처리 실패: {e.Message}");
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