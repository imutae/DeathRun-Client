using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableManager
{
    private static readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();
    private static readonly Dictionary<string, Task<GameObject>> _loadingTasks = new();

    public static async Task<GameObject> LoadPrefabAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is null or empty.");

        if (_prefabHandles.TryGetValue(address, out var cachedHandle))
        {
            if (cachedHandle.IsValid() &&
                cachedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return cachedHandle.Result;
            }
        }

        if (_loadingTasks.TryGetValue(address, out var runningTask))
        {
            return await runningTask;
        }

        var task = LoadPrefabInternalAsync(address);
        _loadingTasks[address] = task;

        try
        {
            return await task;
        }
        finally
        {
            _loadingTasks.Remove(address);
        }
    }

    private static async Task<GameObject> LoadPrefabInternalAsync(string address)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        _prefabHandles[address] = handle;

        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            _prefabHandles.Remove(address);

            if (handle.IsValid())
                Addressables.Release(handle);

            throw new Exception($"Addressable prefab load failed: {address}");
        }

        return handle.Result;
    }

    public static bool IsPrefabLoaded(string address)
    {
        return _prefabHandles.TryGetValue(address, out var handle) &&
               handle.IsValid() &&
               handle.Status == AsyncOperationStatus.Succeeded;
    }

    public static GameObject GetLoadedPrefab(string address)
    {
        if (!_prefabHandles.TryGetValue(address, out var handle) ||
            !handle.IsValid() ||
            handle.Status != AsyncOperationStatus.Succeeded)
        {
            throw new InvalidOperationException($"Prefab is not loaded: {address}");
        }

        return handle.Result;
    }

    public static void ReleasePrefab(string address)
    {
        if (!_prefabHandles.TryGetValue(address, out var handle))
            return;

        _prefabHandles.Remove(address);

        if (handle.IsValid())
            Addressables.Release(handle);
    }

    public static void ReleaseAll()
    {
        foreach (var handle in _prefabHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        _prefabHandles.Clear();
        _loadingTasks.Clear();
    }

    /// <summary>
    /// Unity에서 Domain Reload를 끄고 Play Mode에 들어갈 경우,
    /// static field가 이전 Play 상태를 유지할 수 있어서 초기화용으로 둔다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _prefabHandles.Clear();
        _loadingTasks.Clear();
    }
}