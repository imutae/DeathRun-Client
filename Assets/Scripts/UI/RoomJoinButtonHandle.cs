using TMPro;
using UnityEngine;

public class RoomJoinButtonHandle : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text = null;

    private RoomInfo _roomInfo;
    private bool _hasRoomInfo = false;

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
        _text.text = $"Room {_roomInfo.RoomId} ({_roomInfo.CurrentPlayers}/{ProtocolConstants.MaxRoomPlayers})";
        _hasRoomInfo = true;
    }

    public void OnClickJoinRoomButton()
    {
        if(!_hasRoomInfo)
        {
            Debug.LogWarning("Attempted to join a room without valid room info.");
            return;
        }

        NetworkManager.Instance.JoinRoom(_roomInfo.RoomId);
    }

    private void OnDisable()
    {
        _text.text = string.Empty;
        _roomInfo = new RoomInfo();
        _hasRoomInfo = false;
    }
}
