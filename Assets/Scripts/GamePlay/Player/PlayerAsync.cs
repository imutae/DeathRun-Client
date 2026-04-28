using UnityEngine;

public class PlayerAsync : MonoBehaviour
{
    private float sendInterval = 0.05f;

    private float _elapsed;
    private Vector2 _lastSentPosition;

    private void Start()
    {
        _lastSentPosition = transform.position;
    }

    private void FixedUpdate()
    {
        _elapsed += Time.fixedDeltaTime;

        if (_elapsed < sendInterval)
            return;

        _elapsed = 0f;

        Vector2 currentPosition = transform.position;

        if (Vector2.Distance(_lastSentPosition, currentPosition) < 0.01f)
            return;

        _lastSentPosition = currentPosition;
        NetworkManager.Instance.SendMove(currentPosition);
    }

}
