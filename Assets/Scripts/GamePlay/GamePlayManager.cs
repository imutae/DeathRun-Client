using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    private Dictionary<long, GameObject> _otherPlayers = new Dictionary<long, GameObject>();

    private async void Awake()
    {
        NetworkManager.Instance.OnOtherPlayerJoined += JoinOtherUser;
        NetworkManager.Instance.OnOtherPlayerLeft += ExitOtherUser;
        NetworkManager.Instance.OnMoveReceived += OtherPlayerMoveRecive;

        await PoolManager.Instance.PreloadAsync("OtherCharacter", 7);
    }

    private void Start()
    {
        List<long> userIds = SceneLoadManager.Instance.GetCurrentRoomSessionIdsSnapshot();

        foreach (long userId in userIds)
        {
            JoinOtherUser(userId);
        }
    }

    private async void JoinOtherUser(long userId)
    {
        if (userId == PlayerManager.Instance.PlayerName)
        {
            return;
        }

        if(_otherPlayers.ContainsKey(userId))
        {
            return;
        }

        GameObject otherCharacter = await PoolManager.Instance.SpawnAsync("OtherCharacter", Vector3.zero, Quaternion.identity);
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
        foreach (var otherPlayer in _otherPlayers.Values)
        {
            PoolManager.Instance.Despawn(otherPlayer);
        }

        _otherPlayers.Clear();

        NetworkManager.Instance.OnOtherPlayerJoined -= JoinOtherUser;
        NetworkManager.Instance.OnOtherPlayerLeft -= ExitOtherUser;
        NetworkManager.Instance.OnMoveReceived -= OtherPlayerMoveRecive;

        // Scene ÀüÈ¯
        NetworkManager.Instance.LeaveRoom();
        SceneLoadManager.Instance.LoadLobbyScene();
    }
}
