using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField = null;

    [SerializeField]
    private RectTransform chatContent = null;

    private ChatElement[] chatElement = null;

    private void Start()
    {
        NetworkManager.Instance.OnChatReceived += (chatPacket) =>
        {
            string message = $"{chatPacket.UserName}: {chatPacket.Message}";
            PoolManager.Instance.SpawnAsync(Address.ChatElementPrefab, Vector3.zero, Quaternion.identity, chatContent).Result.GetComponent<ChatElement>().SetChatText(message);
        };
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
