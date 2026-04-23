using UnityEngine;

public class PlayerManager : MonoSingleton<PlayerManager>
{
    private long playerName = 0;
    public long PlayerName => playerName;

    public void SetPlayerName(long name)
    {
        playerName = name;
    }
}
