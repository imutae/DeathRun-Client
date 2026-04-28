using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField = null;

    [SerializeField]
    private RectTransform chatContent = null;

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
            return;

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
        string message = $"{chatPacket.SessionId}: {chatPacket.Message}";

        GameObject chatObject = PoolManager.Instance.Spawn(
            Address.ChatElementPrefab,
            Vector3.zero,
            Quaternion.identity,
            chatContent);

        chatObject.GetComponent<ChatElement>().SetChatText(message);
    }

    public void OnClickSendButton()
    {
        string message = inputField.text;
        if(string.IsNullOrEmpty(message))
        {
            return;
        }

        NetworkManager.Instance.SendChat(message);
        inputField.text = string.Empty;
    }
}
