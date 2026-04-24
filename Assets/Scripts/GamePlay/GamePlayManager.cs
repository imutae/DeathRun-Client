using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    private void Awake()
    {
        
    }

    private void JoinOtherUser(long userId)
    {
        
    }

    public void ExitGame()
    {
        NetworkManager.Instance.LeaveRoom();
        SceneLoadManager.Instance.LoadLobbyScene();
    }
}
