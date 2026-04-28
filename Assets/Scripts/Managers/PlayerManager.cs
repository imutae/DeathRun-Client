using UnityEngine;

public class PlayerManager : MonoSingleton<PlayerManager>
{
    private long sessionId = 0;
    public long LocalSessionId => sessionId;

    public void SetLocalSessionId(long id)
    {
        sessionId = id;
    }
}
