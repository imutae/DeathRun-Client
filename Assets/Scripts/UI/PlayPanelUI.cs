using UnityEngine;

public class PlayPanelUI : MonoBehaviour
{
    [SerializeField]
    private Transform _roomListContentTransform = null;

    [SerializeField]
    private RoomJoinButtonHandle _roomJoinButtonHandle = null;

    private void Start()
    {
        NetworkManager.Instance.OnRoomListReceived += UpdateRoomList;
    }

    private void UpdateRoomList(SRoomListPacket packet)
    {
        RoomJoinButtonHandle[] roomJoinButtons = _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>();
        if(packet.RoomCount >= roomJoinButtons.Length)
        {
            for (int i = roomJoinButtons.Length; i <= packet.RoomCount; i++)
            {
                Instantiate(_roomJoinButtonHandle.gameObject, _roomListContentTransform);
            }
        }

        roomJoinButtons = _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>();

        for (int i = 1; i < roomJoinButtons.Length; i++)
        {
            if(i <= packet.RoomCount)
            {
                RoomInfo roomInfo = packet.Rooms[i];
                roomJoinButtons[i - 1].SetRoomInfo(roomInfo);

                roomJoinButtons[i - 1].gameObject.SetActive(true);
            }
            else
            {
                roomJoinButtons[i - 1].gameObject.SetActive(false);
            }
        }
    }

    public void OnClickRefreshButton()
    {
        NetworkManager.Instance.RequestRoomList();
    }

    public void OnClickCreateRoomButton()
    {
        NetworkManager.Instance.CreateRoom();
    }

    public void SetPanelOpen(bool isOpen)
    {
        gameObject.SetActive(isOpen);
    }
}
