using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField = null;

    [SerializeField]
    private RectTransform chatContent = null;

    private async void Start()
    {
        await PoolManager.Instance.PreloadAsync(Address.ChatElementPrefab, 10);

        NetworkManager.Instance.OnChatReceived += HandleChatReceived;
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

    private void OnDestroy()
    {
        NetworkManager.Instance.OnChatReceived -= HandleChatReceived;
    }
}
