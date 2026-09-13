using UnityEngine;
using Photon.Pun;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public PhotonRoomSearcher RoomSearcher;

    public delegate void OnRoomCallback();
    public OnRoomCallback OnRoom;

    public static PhotonManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
            if (RoomSearcher != null)
            {
                Instance.RoomSearcher = RoomSearcher;
                //Steal room searcher reference from newer instance
            }
            Destroy(this.gameObject);

            return;
        }
        Instance = this;

        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined to Lobby");
        if (Instance.RoomSearcher == null)
        {
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: "new room");
        }
        else { 
            RoomSearcher.FetchRooms();
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to Room");
        string roonName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Room name: " + roonName + " | PlayerCount: " + playerCount);
        OnRoom();
    }

      


}
