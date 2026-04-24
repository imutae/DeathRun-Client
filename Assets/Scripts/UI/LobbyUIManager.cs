using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]
    private PlayPanelUI playPanelUI = null;

    private void Start()
    {
        
    }

    public void SetActivePlayPanel(bool isActive)
    {
        playPanelUI.SetPanelOpen(isActive);
    }
}
