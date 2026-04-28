using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayPanelUI : MonoBehaviour
{
    [SerializeField]
    private Transform _roomListContentTransform = null;

    [SerializeField]
    private RoomJoinButtonHandle _roomJoinButtonHandle = null;

    private NetworkManager _networkManager = null;

    private List<RoomJoinButtonHandle> roomJoinButtons = new List<RoomJoinButtonHandle>();

    private void Awake()
    {
        _networkManager = NetworkManager.Instance;
        roomJoinButtons = _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>(true).ToList();

        if (roomJoinButtons.Count < ProtocolConstants.MaxRoomCount)
        {
            int buttonsToCreate = ProtocolConstants.MaxRoomCount - roomJoinButtons.Count;
            for (int i = 0; i < buttonsToCreate; i++)
            {
                RoomJoinButtonHandle newButton = Instantiate(_roomJoinButtonHandle, _roomListContentTransform);
                newButton.gameObject.SetActive(false);
                roomJoinButtons.Add(newButton);
            }
        }
    }

    private void OnEnable()
    {
        _networkManager.OnRoomListReceived += UpdateRoomList;
    }

    private void OnDisable()
    {
        _networkManager.OnRoomListReceived -= UpdateRoomList;
    }

    private void UpdateRoomList(SRoomListPacket packet)
    {
        if(packet == null)
        {
            Debug.LogError("Received null packet in UpdateRoomList.");
            return;
        }

        if(packet.Rooms == null)
        {
            Debug.LogError("Received packet with null Rooms array in UpdateRoomList.");
            return;
        }

        for (int i = 0; i < ProtocolConstants.MaxRoomCount; i++)
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