using System;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    private TcpClientCore tcpClientCore = null;

    protected override void OnInitialize()
    {
        tcpClientCore = new TcpClientCore();
        tcpClientCore.Connect("127.0.0.1", 7777);
    }

    protected override void OnApplicationQuit()
    {
        tcpClientCore.Disconnect();
        base.OnApplicationQuit();
    }

    public void SendChat(string message)
    {
        if (tcpClientCore.IsConnected)
        {

        }
        else
        {
            Debug.LogWarning("서버에 연결되지 않음");
        }
    }
}
