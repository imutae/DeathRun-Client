using TMPro;
using UnityEngine;

public class ChatElement : MonoBehaviour
{
    [SerializeField]
    private TMP_Text chatText = null;

    public void SetChatText(string text)
    {
        if (chatText != null)
        {
            chatText.text = text;
        }
    }
}
