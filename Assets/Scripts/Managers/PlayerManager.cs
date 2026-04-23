using UnityEngine;

public class PlayerManager : MonoSingleton<PlayerManager>
{
    private string playerName = "";
    public string PlayerName => playerName;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
