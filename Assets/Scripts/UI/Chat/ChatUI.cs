using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private RectTransform chatContent = null;

    private NetworkManager _networkManager = null;
    private bool _isSubscribed = false;
    private bool _isDestroyed = false;

    private async void Start()
    {
        await PoolManager.Instance.PreloadAsync(Address.ChatElementPrefab, 10);

        if (_isDestroyed || this == null)
            return;

        _networkManager = NetworkManager.Instance;

        if (_networkManager == null)
        {
            Debug.LogWarning("[ChatUI] NetworkManager is not available.");
            return;
        }

        _networkManager.OnChatReceived += HandleChatReceived;
        _isSubscribed = true;
    }

    private void OnDestroy()
    {
        _isDestroyed = true;

        if (_isSubscribed && _networkManager != null)
        {
            _networkManager.OnChatReceived -= HandleChatReceived;
            _isSubscribed = false;
        }
    }

    private void HandleChatReceived(ChatPacket chatPacket)
    {
        if (chatPacket == null)
            return;

        if (chatContent == null)
        {
            Debug.LogError("[ChatUI] Chat content is not assigned.");
            return;
        }

        GameObject chatObject = PoolManager.Instance.Spawn(
            Address.ChatElementPrefab,
            Vector3.zero,
            Quaternion.identity,
            chatContent
        );

        if (chatObject == null)
        {
            Debug.LogError("[ChatUI] Failed to spawn chat element.");
            return;
        }

        if (!chatObject.TryGetComponent(out ChatElement chatElement))
        {
            Debug.LogError("[ChatUI] ChatElement component is missing on spawned object.");
            PoolManager.Instance.Despawn(chatObject);
            return;
        }

        string message = $"{chatPacket.SessionId}: {chatPacket.Message}";
        chatElement.SetChatText(message);
    }

    public void OnClickSendButton()
    {
        if (inputField == null)
        {
            Debug.LogError("[ChatUI] InputField is not assigned.");
            return;
        }

        string message = inputField.text;

        if (string.IsNullOrWhiteSpace(message))
            return;

        if (_networkManager == null)
        {
            _networkManager = NetworkManager.Instance;
        }

        if (_networkManager == null)
        {
            Debug.LogWarning("[ChatUI] NetworkManager is not available.");
            return;
        }

        _networkManager.SendChat(message);

        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }
}
