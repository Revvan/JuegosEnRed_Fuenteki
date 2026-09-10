using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class PhotonRoomSearcher : MonoBehaviourPunCallbacks
{
    [SerializeField] UI_RoomLIst ui_RoomLIst;

    public List<RoomInfo> local_roomList;



    public void FetchRooms()
    {
        Debug.Log("RefreshButton");

        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        local_roomList = roomList;
        Debug.Log(local_roomList.Count);
        if (local_roomList.Count == 0)
        {
            ui_RoomLIst.Refresh_List();
            return;
        }
        foreach (RoomInfo roomInfo in local_roomList)
        {
            Debug.Log(roomInfo.Name);
        }

        ui_RoomLIst.Refresh_List();
        
    }
}
