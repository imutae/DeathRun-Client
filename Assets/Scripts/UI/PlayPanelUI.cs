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

        if (_roomListContentTransform == null)
        {
            Debug.LogError("[PlayPanelUI] Room list content transform is not assigned.");
            return;
        }

        if (_roomJoinButtonHandle == null)
        {
            Debug.LogError("[PlayPanelUI] Room join button prefab is not assigned.");
            return;
        }

        roomJoinButtons = _roomListContentTransform
            .GetComponentsInChildren<RoomJoinButtonHandle>(true)
            .ToList();

        // 기존 버튼 생성 로직 유지
    }

    private void OnEnable()
    {
        if (_networkManager == null)
            _networkManager = NetworkManager.Instance;

        if (_networkManager == null)
            return;

        _networkManager.OnRoomListReceived += UpdateRoomList;
    }

    private void OnDisable()
    {
        if (_networkManager != null)
        {
            _networkManager.OnRoomListReceived -= UpdateRoomList;
        }
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