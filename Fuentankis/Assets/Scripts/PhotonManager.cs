using UnityEngine;
using Photon.Pun;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public delegate void OnRoomCallback();
    public OnRoomCallback OnRoom;

    public static PhotonManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
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
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: "new room esteban");
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
