using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SimpleTcpClient : MonoBehaviour
{
    private TcpClient _client;
    private NetworkStream _stream;

    private string _ip = "127.0.0.1";
    private int _port = 7777;

    void Start()
    {
        Connect();
    }

    void Update()
    {
        // Space 키 입력 시 패킷 전송
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendPacket("Hello from Unity!", 1);
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Connect()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(_ip, _port);

            if (_client.Connected)
            {
                _stream = _client.GetStream();
                Debug.Log("서버 연결 성공!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 연결 실패: {e.Message}");
        }
    }

    private void SendPacket(string message, ushort packetId)
    {
        if (_client == null || !_client.Connected)
        {
            Debug.LogWarning("서버에 연결되지 않음");
            return;
        }

        try
        {
            byte[] packet = MakePacket(message, packetId);
            _stream.Write(packet, 0, packet.Length);

            Debug.Log($"패킷 전송 (ID: {packetId}, Size: {packet.Length})");
        }
        catch (Exception e)
        {
            Debug.LogError($"전송 실패: {e.Message}");
        }
    }

    /// <summary>
    /// [size(2)][id(2)][body] 형태 패킷 생성
    /// little-endian 기준
    /// </summary>
    private byte[] MakePacket(string message, ushort packetId)
    {
        byte[] body = Encoding.UTF8.GetBytes(message);

        ushort size = (ushort)(4 + body.Length); // header(4) + body

        byte[] packet = new byte[size];

        // size (2 bytes)
        packet[0] = (byte)(size & 0xFF);
        packet[1] = (byte)((size >> 8) & 0xFF);

        // id (2 bytes)
        packet[2] = (byte)(packetId & 0xFF);
        packet[3] = (byte)((packetId >> 8) & 0xFF);

        // body
        Buffer.BlockCopy(body, 0, packet, 4, body.Length);

        return packet;
    }

    private void Disconnect()
    {
        try
        {
            _stream?.Close();
            _client?.Close();

            Debug.Log("서버 연결 종료");
        }
        catch (Exception e)
        {
            Debug.LogError($"종료 중 오류: {e.Message}");
        }
    }
}