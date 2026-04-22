using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField = null;

    //[SerializeField]
    //private RectTransform chatContent = null;

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
