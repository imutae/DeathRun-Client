using UnityEngine;

public class PlayPanelUI : MonoBehaviour
{
    [SerializeField]
    private Transform _roomListContentTransform = null;

    [SerializeField]
    private RoomJoinButtonHandle _roomJoinButtonHandle = null;

    private void OnEnable()
    {
        NetworkManager.Instance.OnRoomListReceived += UpdateRoomList;
    }

    private void OnDisable()
    {
        if (NetworkManager.Instance == null)
            return;

        NetworkManager.Instance.OnRoomListReceived -= UpdateRoomList;
    }

    private void UpdateRoomList(SRoomListPacket packet)
    {
        RoomJoinButtonHandle[] roomJoinButtons =
            _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>(true);

        while (roomJoinButtons.Length < packet.RoomCount)
        {
            Instantiate(_roomJoinButtonHandle.gameObject, _roomListContentTransform);

            roomJoinButtons =
                _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>(true);
        }

        for (int i = 0; i < roomJoinButtons.Length; i++)
        {
            if (i < packet.RoomCount)
            {
                RoomInfo roomInfo = packet.Rooms[i];
                roomJoinButtons[i].SetRoomInfo(roomInfo);
                roomJoinButtons[i].gameObject.SetActive(true);
            }
            else
            {
                roomJoinButtons[i].gameObject.SetActive(false);
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