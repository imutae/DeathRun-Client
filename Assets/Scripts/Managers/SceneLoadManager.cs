using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DeathRunSceneType
{
    Unknown,
    Lobby,
    Game
}

public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    public const string LobbySceneName = "LobbyScene";
    public const string GameSceneName = "GameScene";

    private readonly List<long> currentRoomSessionIds = new List<long>();

    public IReadOnlyList<long> CurrentRoomSessionIds => currentRoomSessionIds;
    public DeathRunSceneType CurrentSceneType { get; private set; } = DeathRunSceneType.Unknown;
    public bool IsLoading { get; private set; }

    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoadCompleted;

    protected override void OnInitialize()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;

        string activeSceneName = SceneManager.GetActiveScene().name;
        CurrentSceneType = GetSceneType(activeSceneName);
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        base.OnDestroy();
    }

    public void LoadGameScene(SJoinPacket joinPacket)
    {
        if (joinPacket == null)
        {
            Debug.LogWarning("LoadGameScene 실패: joinPacket이 null입니다.");
            return;
        }

        if (!joinPacket.IsSuccess)
        {
            Debug.LogWarning("LoadGameScene 실패: S_JOIN 결과가 실패입니다.");
            return;
        }

        CacheRoomSessionIds(joinPacket);
        LoadScene(GameSceneName);
    }

    public void LoadLobbyScene(bool clearRoomState = true)
    {
        if (clearRoomState)
            ClearRoomState();

        LoadScene(LobbySceneName);
    }

    public void LeaveRoomAndLoadLobby()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.LeaveRoom();

        LoadLobbyScene(true);
    }

    public bool IsLocalPlayer(long sessionId)
    {
        if (PlayerManager.Instance == null)
            return false;

        return PlayerManager.Instance.LocalSessionId == sessionId;
    }

    public List<long> GetCurrentRoomSessionIdsSnapshot()
    {
        return new List<long>(currentRoomSessionIds);
    }

    public void ClearRoomState()
    {
        currentRoomSessionIds.Clear();
    }

    private void CacheRoomSessionIds(SJoinPacket joinPacket)
    {
        currentRoomSessionIds.Clear();

        int count = Mathf.Min(joinPacket.PlayerCount, ProtocolConstants.MaxRoomPlayers);

        for (int i = 0; i < count; i++)
        {
            long sessionId = joinPacket.SessionIds[i];

            if (sessionId == 0)
                continue;

            currentRoomSessionIds.Add(sessionId);
        }
    }

    private void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"이미 씬 로딩 중입니다. 요청 무시: {sceneName}");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        IsLoading = true;
        OnSceneLoadStarted?.Invoke(sceneName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"씬 로드 실패: {sceneName}");
            IsLoading = false;
            yield break;
        }

        while (!operation.isDone)
            yield return null;

        IsLoading = false;
        OnSceneLoadCompleted?.Invoke(sceneName);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CurrentSceneType = GetSceneType(scene.name);
    }

    private DeathRunSceneType GetSceneType(string sceneName)
    {
        if (sceneName == LobbySceneName)
            return DeathRunSceneType.Lobby;

        if (sceneName == GameSceneName)
            return DeathRunSceneType.Game;

        return DeathRunSceneType.Unknown;
    }
}