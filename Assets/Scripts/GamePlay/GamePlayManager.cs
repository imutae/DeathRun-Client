using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    private Dictionary<long, GameObject> _otherPlayers = new Dictionary<long, GameObject>();

    private void Awake()
    {
        NetworkManager.Instance.OnOtherPlayerJoined += JoinOtherUser;
        NetworkManager.Instance.OnOtherPlayerLeft += ExitOtherUser;
        NetworkManager.Instance.OnMoveReceived += OtherPlayerMoveRecive;

        PoolManager.Instance.PreloadAsync("OtherCharacter", 7).Wait();
    }    

    private void JoinOtherUser(long userId)
    {
        if (userId == PlayerManager.Instance.PlayerName)
        {
            return;
        }

        GameObject otherCharacter = PoolManager.Instance.Spawn("OtherCharacter", Vector3.zero, Quaternion.identity);
        _otherPlayers.Add(userId, otherCharacter);
    }

    private void ExitOtherUser(long userId)
    {
        if (userId == PlayerManager.Instance.PlayerName)
        {
            return;
        }

        if (_otherPlayers.TryGetValue(userId, out GameObject otherCharacter))
        {
            PoolManager.Instance.Despawn(otherCharacter);
            _otherPlayers.Remove(userId);
        }
    }

    private void OtherPlayerMoveRecive(SMovePacket movePacket)
    {
        if (_otherPlayers.TryGetValue(movePacket.SessionId, out GameObject otherCharacter))
        {
            Vector2 newPosition = new Vector2(movePacket.X, movePacket.Y);
            otherCharacter.transform.position = newPosition;
        }
    }

    public void ExitGame()
    {
        // Callback 초기화
        NetworkManager.Instance.OnOtherPlayerJoined -= JoinOtherUser;
        NetworkManager.Instance.OnOtherPlayerLeft -= ExitOtherUser;

        // Scene 전환
        NetworkManager.Instance.LeaveRoom();
        SceneLoadManager.Instance.LoadLobbyScene();
    }
}
