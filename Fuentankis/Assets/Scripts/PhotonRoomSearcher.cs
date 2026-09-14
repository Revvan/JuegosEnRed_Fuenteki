using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class PhotonRoomSearcher : MonoBehaviourPunCallbacks
{
    [SerializeField] UI_RoomLIst ui_RoomLIst;

    public List<RoomInfo> local_roomList;
    private readonly Dictionary<string, RoomInfo> cachedRooms = new Dictionary<string, RoomInfo>();
    private bool uiReady;

    private void Awake()
    {
        // The menu may receive the initial list before this scene is loaded.
        UpdateRoomCache(UI_MainMenu.TakeInitialRooms());
    }

    private IEnumerator Start()
    {
        yield return null; // Let the scene controls finish initializing.
        uiReady = true;
        ui_RoomLIst.Refresh_List();
    }
    public void FetchRooms()
    {
        if (uiReady) ui_RoomLIst.Refresh_List(); // Photon pushes room changes automatically.
    }

    public override void OnLeftLobby()
    {
        cachedRooms.Clear();
        local_roomList = new List<RoomInfo>();
        if (uiReady) ui_RoomLIst.Refresh_List();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomCache(roomList);
        if (uiReady)
            ui_RoomLIst.Refresh_List();
    }

    private void UpdateRoomCache(List<RoomInfo> roomList)
    {
        // Photon sends changes, not the complete list on every callback.
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) cachedRooms.Remove(room.Name);
            else cachedRooms[room.Name] = room;
        }
        local_roomList = new List<RoomInfo>(cachedRooms.Values);
    }
}
