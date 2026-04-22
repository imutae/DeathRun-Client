using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class PoolManager : MonoSingleton<PoolManager>
{
    private sealed class Pool
    {
        public readonly string Address;
        public readonly GameObject Prefab;
        public readonly Transform Root;

        public readonly Stack<GameObject> InactiveObjects = new Stack<GameObject>();
        public readonly HashSet<GameObject> ActiveObjects = new HashSet<GameObject>();

        public int ActiveCount => ActiveObjects.Count;
        public int InactiveCount => InactiveObjects.Count;
        public int TotalCount => ActiveObjects.Count + InactiveObjects.Count;

        public Pool(string address, GameObject prefab, Transform root)
        {
            Address = address;
            Prefab = prefab;
            Root = root;
        }
    }

    private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();
    private readonly Dictionary<string, Task<Pool>> _creatingPoolTasks = new Dictionary<string, Task<Pool>>();

    private readonly Dictionary<GameObject, Pool> _ownerByInstance = new Dictionary<GameObject, Pool>();
    private readonly Dictionary<GameObject, IPoolable[]> _poolablesByInstance = new Dictionary<GameObject, IPoolable[]>();

    /// <summary>
    /// address에 해당하는 prefab을 AddressableManager로 로드하고,
    /// count 개수만큼 미리 생성해서 풀에 넣는다.
    /// 
    /// count는 "추가 생성 개수"가 아니라 "최소 보유 개수"다.
    /// 예: 이미 5개 있으면 PreloadAsync(address, 10)은 5개만 추가 생성한다.
    /// </summary>
    public async Task PreloadAsync(string address, int count)
    {
        ValidateAddress(address);

        if (count <= 0)
            return;

        Pool pool = await GetOrCreatePoolAsync(address);

        int createCount = count - pool.TotalCount;

        for (int i = 0; i < createCount; i++)
        {
            GameObject instance = CreateInstance(pool);
            pool.InactiveObjects.Push(instance);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼낸다.
    /// 풀에 남은 비활성 오브젝트가 없으면 새로 Instantiate한다.
    /// </summary>
    public async Task<GameObject> SpawnAsync(
        string address,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        ValidateAddress(address);

        Pool pool = await GetOrCreatePoolAsync(address);
        GameObject instance = RentInstance(pool);

        pool.ActiveObjects.Add(instance);

        Transform tr = instance.transform;
        tr.SetParent(parent, true);
        tr.SetPositionAndRotation(position, rotation);

        instance.SetActive(true);
        InvokeSpawned(instance);

        return instance;
    }

    /// <summary>
    /// 회전값 없이 생성할 때 사용하는 편의 메서드.
    /// </summary>
    public Task<GameObject> SpawnAsync(
        string address,
        Vector3 position,
        Transform parent = null)
    {
        return SpawnAsync(address, position, Quaternion.identity, parent);
    }

    /// <summary>
    /// 오브젝트를 삭제하지 않고 풀에 반납한다.
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null)
            return;

        if (!_ownerByInstance.TryGetValue(instance, out Pool pool))
        {
            // 이 PoolManager가 만든 오브젝트가 아니면 안전하게 Destroy 처리.
            Destroy(instance);
            return;
        }

        // 이미 반납된 오브젝트를 중복 Despawn하는 경우 방지.
        if (!pool.ActiveObjects.Remove(instance))
            return;

        InvokeDespawned(instance);

        instance.SetActive(false);

        Transform tr = instance.transform;
        tr.SetParent(pool.Root, false);
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;

        pool.InactiveObjects.Push(instance);
    }

    /// <summary>
    /// 특정 Addressable 주소의 활성 오브젝트를 전부 풀에 반납한다.
    /// Destroy는 하지 않는다.
    /// </summary>
    public void DespawnAll(string address)
    {
        ValidateAddress(address);

        if (!_pools.TryGetValue(address, out Pool pool))
            return;

        List<GameObject> activeObjects = new List<GameObject>(pool.ActiveObjects);

        for (int i = 0; i < activeObjects.Count; i++)
        {
            Despawn(activeObjects[i]);
        }
    }

    /// <summary>
    /// 특정 풀을 완전히 제거한다.
    /// 
    /// forceClearActiveObjects가 false인데 활성 오브젝트가 남아 있으면 예외를 던진다.
    /// forceClearActiveObjects가 true면 활성/비활성 오브젝트 모두 Destroy한다.
    /// 
    /// 마지막에 AddressableManager.ReleasePrefab(address)를 호출한다.
    /// </summary>
    public void ClearPool(string address, bool forceClearActiveObjects = false)
    {
        ValidateAddress(address);

        if (!_pools.TryGetValue(address, out Pool pool))
            return;

        if (pool.ActiveObjects.Count > 0 && !forceClearActiveObjects)
        {
            throw new InvalidOperationException(
                $"Pool has active objects. Address: {address}, ActiveCount: {pool.ActiveObjects.Count}");
        }

        ClearInactiveObjects(pool);
        ClearActiveObjects(pool);

        if (pool.Root != null)
            Destroy(pool.Root.gameObject);

        _pools.Remove(address);
        _creatingPoolTasks.Remove(address);

        AddressableManager.ReleasePrefab(address);
    }

    /// <summary>
    /// 모든 풀을 완전히 제거한다.
    /// 보통 스테이지 종료, 씬 종료, 게임 종료 시 호출한다.
    /// </summary>
    public void ClearAllPools(bool forceClearActiveObjects = true)
    {
        List<string> addresses = new List<string>(_pools.Keys);

        for (int i = 0; i < addresses.Count; i++)
        {
            ClearPool(addresses[i], forceClearActiveObjects);
        }
    }

    public bool HasPool(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        return _pools.ContainsKey(address);
    }

    public int GetActiveCount(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return 0;

        return _pools.TryGetValue(address, out Pool pool)
            ? pool.ActiveCount
            : 0;
    }

    public int GetInactiveCount(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return 0;

        return _pools.TryGetValue(address, out Pool pool)
            ? pool.InactiveCount
            : 0;
    }

    public int GetTotalCount(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return 0;

        return _pools.TryGetValue(address, out Pool pool)
            ? pool.TotalCount
            : 0;
    }

    private async Task<Pool> GetOrCreatePoolAsync(string address)
    {
        if (_pools.TryGetValue(address, out Pool existingPool))
            return existingPool;

        if (_creatingPoolTasks.TryGetValue(address, out Task<Pool> runningTask))
            return await runningTask;

        Task<Pool> task = CreatePoolAsync(address);
        _creatingPoolTasks[address] = task;

        try
        {
            return await task;
        }
        finally
        {
            _creatingPoolTasks.Remove(address);
        }
    }

    private async Task<Pool> CreatePoolAsync(string address)
    {
        GameObject prefab = await AddressableManager.LoadPrefabAsync(address);

        // 동시에 같은 address 풀 생성 요청이 들어왔을 때 중복 생성 방지.
        if (_pools.TryGetValue(address, out Pool existingPool))
            return existingPool;

        GameObject rootObject = new GameObject($"[Pool] {address}");
        rootObject.transform.SetParent(transform, false);

        // 루트를 비활성화해두면 active prefab을 Instantiate해도 OnEnable이 바로 실행되는 일을 줄일 수 있다.
        rootObject.SetActive(false);

        Pool pool = new Pool(address, prefab, rootObject.transform);
        _pools.Add(address, pool);

        return pool;
    }

    private GameObject RentInstance(Pool pool)
    {
        while (pool.InactiveObjects.Count > 0)
        {
            GameObject instance = pool.InactiveObjects.Pop();

            if (instance != null)
                return instance;

            RemoveInstanceRecord(instance);
        }

        return CreateInstance(pool);
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.Prefab, pool.Root);

        instance.name = pool.Prefab.name;
        instance.SetActive(false);

        _ownerByInstance[instance] = pool;

        // 자식까지 포함해서 IPoolable을 캐싱한다.
        // Spawn/Despawn마다 GetComponents를 호출하지 않기 위함.
        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        _poolablesByInstance[instance] = poolables;

        return instance;
    }

    private void ClearInactiveObjects(Pool pool)
    {
        while (pool.InactiveObjects.Count > 0)
        {
            GameObject instance = pool.InactiveObjects.Pop();
            DestroyPooledInstance(instance);
        }
    }

    private void ClearActiveObjects(Pool pool)
    {
        List<GameObject> activeObjects = new List<GameObject>(pool.ActiveObjects);

        for (int i = 0; i < activeObjects.Count; i++)
        {
            DestroyPooledInstance(activeObjects[i]);
        }

        pool.ActiveObjects.Clear();
    }

    private void DestroyPooledInstance(GameObject instance)
    {
        RemoveInstanceRecord(instance);

        if (instance != null)
            Destroy(instance);
    }

    private void RemoveInstanceRecord(GameObject instance)
    {
        if (instance == null)
            return;

        _ownerByInstance.Remove(instance);
        _poolablesByInstance.Remove(instance);
    }

    private void InvokeSpawned(GameObject instance)
    {
        if (!_poolablesByInstance.TryGetValue(instance, out IPoolable[] poolables))
            return;

        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i]?.OnSpawned();
        }
    }

    private void InvokeDespawned(GameObject instance)
    {
        if (!_poolablesByInstance.TryGetValue(instance, out IPoolable[] poolables))
            return;

        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i]?.OnDespawned();
        }
    }

    private static void ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Addressable address is null or empty.");
    }
}