using System;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    private const string OtherCharacterAddress = "OtherCharacter";
    private const int OtherCharacterPreloadCount = 7;

    private readonly Dictionary<long, GameObject> _otherPlayers = new Dictionary<long, GameObject>();
    private readonly HashSet<long> _pendingSpawnSessionIds = new HashSet<long>();
    private readonly HashSet<long> _cancelledSpawnSessionIds = new HashSet<long>();

    private NetworkManager _networkManager = null;
    private bool _isSubscribed = false;
    private bool _isDestroyed = false;
    private bool _isExiting = false;

    private async void Awake()
    {
        SubscribeNetworkEvents();

        try
        {
            await PoolManager.Instance.PreloadAsync(OtherCharacterAddress, OtherCharacterPreloadCount);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GamePlayManager] Failed to preload other character pool: {e.Message}");
        }
    }

    private void Start()
    {
        var userIds = SceneLoadManager.Instance.GetCurrentRoomSessionIdsSnapshot();

        foreach (long userId in userIds)
        {
            JoinOtherUser(userId);
        }
    }

    private void OnDestroy()
    {
        _isDestroyed = true;

        UnsubscribeNetworkEvents();
        DespawnAllOtherPlayers();

        _pendingSpawnSessionIds.Clear();
        _cancelledSpawnSessionIds.Clear();
    }

    private void SubscribeNetworkEvents()
    {
        if (_isSubscribed)
            return;

        _networkManager = NetworkManager.Instance;

        if (_networkManager == null)
        {
            Debug.LogWarning("[GamePlayManager] NetworkManager is not available.");
            return;
        }

        _networkManager.OnOtherPlayerJoined += JoinOtherUser;
        _networkManager.OnOtherPlayerLeft += ExitOtherUser;
        _networkManager.OnMoveReceived += HandleOtherPlayerMoveReceived;

        _isSubscribed = true;
    }

    private void UnsubscribeNetworkEvents()
    {
        if (!_isSubscribed || _networkManager == null)
            return;

        _networkManager.OnOtherPlayerJoined -= JoinOtherUser;
        _networkManager.OnOtherPlayerLeft -= ExitOtherUser;
        _networkManager.OnMoveReceived -= HandleOtherPlayerMoveReceived;

        _isSubscribed = false;
    }

    private async void JoinOtherUser(long userId)
    {
        if (!CanSpawnOtherPlayer(userId))
            return;

        _pendingSpawnSessionIds.Add(userId);
        _cancelledSpawnSessionIds.Remove(userId);

        GameObject otherCharacter = null;

        try
        {
            otherCharacter = await PoolManager.Instance.SpawnAsync(
                OtherCharacterAddress,
                Vector3.zero,
                Quaternion.identity
            );

            if (_isDestroyed || _cancelledSpawnSessionIds.Contains(userId))
            {
                DespawnIfNeeded(otherCharacter);
                return;
            }

            if (otherCharacter == null)
            {
                Debug.LogError($"[GamePlayManager] Failed to spawn other player. SessionId={userId}");
                return;
            }

            if (_otherPlayers.ContainsKey(userId))
            {
                DespawnIfNeeded(otherCharacter);
                return;
            }

            _otherPlayers.Add(userId, otherCharacter);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GamePlayManager] Failed to join other player. SessionId={userId}, Error={e.Message}");
            DespawnIfNeeded(otherCharacter);
        }
        finally
        {
            _pendingSpawnSessionIds.Remove(userId);
            _cancelledSpawnSessionIds.Remove(userId);
        }
    }

    private bool CanSpawnOtherPlayer(long userId)
    {
        if (_isDestroyed)
            return false;

        if (userId == PlayerManager.Instance.LocalSessionId)
            return false;

        if (_otherPlayers.ContainsKey(userId))
            return false;

        if (_pendingSpawnSessionIds.Contains(userId))
            return false;

        return true;
    }

    private void ExitOtherUser(long userId)
    {
        if (userId == PlayerManager.Instance.LocalSessionId)
            return;

        if (_pendingSpawnSessionIds.Contains(userId))
        {
            _cancelledSpawnSessionIds.Add(userId);
        }

        if (_otherPlayers.TryGetValue(userId, out GameObject otherCharacter))
        {
            DespawnIfNeeded(otherCharacter);
            _otherPlayers.Remove(userId);
        }
    }

    private void HandleOtherPlayerMoveReceived(SMovePacket movePacket)
    {
        if (movePacket == null)
            return;

        if (movePacket.SessionId == PlayerManager.Instance.LocalSessionId)
            return;

        if (_otherPlayers.TryGetValue(movePacket.SessionId, out GameObject otherCharacter))
        {
            Vector2 newPosition = new Vector2(movePacket.X, movePacket.Y);
            otherCharacter.transform.position = newPosition;
        }
    }

    public void ExitGame()
    {
        if (_isExiting)
            return;

        _isExiting = true;

        UnsubscribeNetworkEvents();
        DespawnAllOtherPlayers();

        if (_networkManager == null)
        {
            _networkManager = NetworkManager.Instance;
        }

        if (_networkManager != null)
        {
            _networkManager.LeaveRoom();
        }

        SceneLoadManager.Instance.LoadLobbyScene();
    }

    private void DespawnAllOtherPlayers()
    {
        foreach (GameObject otherPlayer in _otherPlayers.Values)
        {
            DespawnIfNeeded(otherPlayer);
        }

        _otherPlayers.Clear();
    }

    private void DespawnIfNeeded(GameObject gameObject)
    {
        if (gameObject == null)
            return;

        PoolManager.Instance.Despawn(gameObject);
    }
}
