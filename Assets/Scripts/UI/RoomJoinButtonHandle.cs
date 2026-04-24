using TMPro;
using UnityEngine;

public class RoomJoinButtonHandle : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text = null;

    private RoomInfo _roomInfo;

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
        _text.text = $"Room {_roomInfo.RoomId} ({_roomInfo.CurrentPlayers}/{ProtocolConstants.MaxRoomPlayers})";
    }

    public void OnClickJoinRoomButton()
    {
        NetworkManager.Instance.JoinRoom(_roomInfo.RoomId);
    }
}
