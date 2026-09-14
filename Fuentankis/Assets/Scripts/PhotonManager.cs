using UnityEngine;
using Photon.Pun;

/// <summary>
/// Central manager for Photon networking.
///
/// Handles the connection flow, lobby and room callbacks,
/// room joining, and communication between Photon and
/// other gameplay systems.
/// </summary>

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public PhotonRoomSearcher RoomSearcher;

    public delegate void OnRoomCallback();
    public OnRoomCallback OnRoom;

    public static PhotonManager Instance;
    // Results UI owns the return flow while this gameplay scene is being unloaded.
    public bool LeavingGameplay { get; set; }
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

        // Reuse an existing Photon connection when coming from the Main Menu.
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Connected, but not currently in a Lobby or Room.
            if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
                PhotonNetwork.JoinLobby();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            // No Photon connection exists yet. This also allows this scene to work when launched directly for testing.
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    public override void OnConnectedToMaster()
    {
        if (LeavingGameplay) return;
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby()
    {
        if (LeavingGameplay) return;
        Debug.Log("Joined to Lobby");
        if (Instance.RoomSearcher == null)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: "new room",
                roomOptions: gameManager != null ? gameManager.CreateRoomOptions() : null);
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
        OnRoom?.Invoke();
    }

      


}
