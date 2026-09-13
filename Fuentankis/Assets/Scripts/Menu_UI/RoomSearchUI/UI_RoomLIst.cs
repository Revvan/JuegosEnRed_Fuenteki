using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;


public class UI_RoomLIst : MonoBehaviour
{
    [SerializeField] PhotonRoomSearcher Searcher;
    [SerializeField] GameObject ButtomPrefab;


    public List<GameObject> RoomButtomList;

    public void Start()
    {
        //Validate
        if (Searcher== null)
            Debug.Log(name + ": PhotonRoomSearcherm not found");
        if (ButtomPrefab== null)
            Debug.Log(name + ": ButtomPrefab not found");

        RoomButtomList = new List<GameObject>();
    }

    public void Refresh_List()
    {
        if (RoomButtomList.Count > 0)
        {
            foreach(GameObject button in RoomButtomList)
            {
                Destroy(button);
            }
            RoomButtomList.Clear();
        }

        if (Searcher.local_roomList.Count == 0)
        {
            return;
        }
        foreach (RoomInfo room in Searcher.local_roomList)
        {
            
            GameObject newButtom = Instantiate(ButtomPrefab, this.transform);
            UI_RoomButtom RoomButtom = newButtom.GetComponent<UI_RoomButtom>();
            RoomButtom.roomName = room.Name;
            RoomButtom.Setup();
        }
    }
}
