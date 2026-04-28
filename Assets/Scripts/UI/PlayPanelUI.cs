using System.Collections.Generic;
using UnityEngine;

public class PlayPanelUI : MonoBehaviour
{
    [SerializeField] private Transform _roomListContentTransform = null;
    [SerializeField] private RoomJoinButtonHandle _roomJoinButtonHandle = null;

    private readonly List<RoomJoinButtonHandle> _roomJoinButtons = new List<RoomJoinButtonHandle>();

    private NetworkManager _networkManager = null;
    private bool _isSubscribed = false;

    private void Awake()
    {
        CacheExistingButtons();
    }

    private void OnEnable()
    {
        NetworkManager networkManager = GetNetworkManager();

        if (networkManager == null)
            return;

        if (_isSubscribed)
            return;

        networkManager.OnRoomListReceived += UpdateRoomList;
        _isSubscribed = true;
    }

    private void OnDisable()
    {
        if (!_isSubscribed || _networkManager == null)
            return;

        _networkManager.OnRoomListReceived -= UpdateRoomList;
        _isSubscribed = false;
    }

    private void CacheExistingButtons()
    {
        _roomJoinButtons.Clear();

        if (_roomListContentTransform == null)
        {
            Debug.LogError("[PlayPanelUI] Room list content transform is not assigned.");
            return;
        }

        RoomJoinButtonHandle[] existingButtons =
            _roomListContentTransform.GetComponentsInChildren<RoomJoinButtonHandle>(true);

        _roomJoinButtons.AddRange(existingButtons);
    }

    private void EnsureRoomButtonCount(int count)
    {
        if (count <= _roomJoinButtons.Count)
            return;

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

        while (_roomJoinButtons.Count < count)
        {
            RoomJoinButtonHandle button = Instantiate(
                _roomJoinButtonHandle,
                _roomListContentTransform
            );

            button.gameObject.SetActive(false);
            _roomJoinButtons.Add(button);
        }
    }

    private void UpdateRoomList(SRoomListPacket packet)
    {
        if (packet == null)
        {
            Debug.LogError("[PlayPanelUI] Received null packet in UpdateRoomList.");
            return;
        }

        if (packet.Rooms == null)
        {
            Debug.LogError("[PlayPanelUI] Received packet with null Rooms array in UpdateRoomList.");
            return;
        }

        int roomCount = Mathf.Clamp(
            packet.RoomCount,
            0,
            Mathf.Min(packet.Rooms.Length, ProtocolConstants.MaxRoomCount)
        );

        EnsureRoomButtonCount(roomCount);

        for (int i = 0; i < _roomJoinButtons.Count; i++)
        {
            RoomJoinButtonHandle button = _roomJoinButtons[i];

            if (button == null)
                continue;

            bool hasRoom = i < roomCount;

            if (hasRoom)
            {
                button.SetRoomInfo(packet.Rooms[i]);
            }

            button.gameObject.SetActive(hasRoom);
        }
    }

    public void OnClickRefreshButton()
    {
        NetworkManager networkManager = GetNetworkManager();

        if (networkManager == null)
            return;

        networkManager.RequestRoomList();
    }

    public void OnClickCreateRoomButton()
    {
        NetworkManager networkManager = GetNetworkManager();

        if (networkManager == null)
            return;

        networkManager.CreateRoom();
    }

    public void SetPanelOpen(bool isOpen)
    {
        gameObject.SetActive(isOpen);
    }

    private NetworkManager GetNetworkManager()
    {
        if (_networkManager != null)
            return _networkManager;

        _networkManager = NetworkManager.Instance;

        if (_networkManager == null)
        {
            Debug.LogWarning("[PlayPanelUI] NetworkManager is not available.");
        }

        return _networkManager;
    }
}
