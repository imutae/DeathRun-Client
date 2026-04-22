public interface IPoolable
{
    /// <summary>
    /// PoolManager.SpawnAsync로 풀에서 꺼내졌을 때 호출된다.
    /// 위치/회전/부모 설정과 SetActive(true) 이후 호출된다.
    /// </summary>
    void OnSpawned();

    /// <summary>
    /// PoolManager.Despawn으로 풀에 반납되기 직전에 호출된다.
    /// SetActive(false) 이전 호출된다.
    /// </summary>
    void OnDespawned();
}