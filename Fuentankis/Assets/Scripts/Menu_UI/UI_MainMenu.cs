using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu UI and manages the Photon flow required
/// to connect, join the Lobby and open the Rooms scene.
/// </summary>

public class UI_MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Scene references")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text connectionStatus;
    [SerializeField] private Button roomsButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private string roomsSceneName = "RoomsScene";

    private bool openingRooms; // Prevents multiple connection / scene-loading requests while the Rooms flow is already in progress.

    // Temporarily stores Room updates received while transitioning
    // from the Main Menu to the Rooms scene.
    // Photon may send the first Room list before RoomsScene finishes loading.
    private static readonly Dictionary<string, RoomInfo> initialRooms = new Dictionary<string, RoomInfo>();

    private void Start()
    {
        playerNameInput.SetTextWithoutNotify(PlayerIdentity.LoadName());
        SetStatus("Elegí ROOMS para conectarte y buscar salas.", true, false);
    }

    public void SavePlayerName(string value)
    {
        playerNameInput.SetTextWithoutNotify(PlayerIdentity.SaveName(value));
    }

    public void EditPlayerName()
    {
        playerNameInput.Select();
        playerNameInput.ActivateInputField();
    }

    public void Connect()
    {
        // Preserve the existing Inspector binding of the retry button.
        OpenRooms();
    }

    /// <summary>
    /// Saves the player name and starts the flow required
    /// to reach the Rooms scene.
    /// </summary>
    public void OpenRooms()
    {
        if (openingRooms)
            return;

        SavePlayerName(playerNameInput.text);
        if (!Application.CanStreamedLevelBeLoaded(roomsSceneName))
        {
            SetStatus("La escena de salas no está disponible en el Build Profile.", true, false);
            return;
        }

        openingRooms = true;
        if (PhotonNetwork.InLobby)
        {
            LoadRoomsScene();
            return;
        }

        initialRooms.Clear();
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            JoinLobby();
            return;
        }

        SetStatus("Conectando a Photon...", false, false);
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.ConnectUsingSettings())
            ConnectionFailed("No se pudo iniciar la conexión. Reintentá.");
    }

    /// <summary>
    /// Joins the Photon Lobby after a successful connection.
    /// </summary>
    private void JoinLobby()
    {
        SetStatus("Conectado. Entrando al lobby...", false, false);
        if (!PhotonNetwork.JoinLobby())
            ConnectionFailed("No se pudo entrar al lobby. Reintentá.");
    }

    private void LoadRoomsScene()
    {
        SetStatus("Abriendo salas...", false, false);
        SceneManager.LoadSceneAsync(roomsSceneName);
    }

    public void Quit()
    {
        SavePlayerName(playerNameInput.text);
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnConnectedToMaster()
    {
        if (openingRooms)
            JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (openingRooms)
            LoadRoomsScene();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) initialRooms.Remove(room.Name);
            else initialRooms[room.Name] = room;
        }
    }

    /// <summary>
    /// Returns any Room updates received before RoomsScene finished loading.
    /// </summary>
    public static List<RoomInfo> TakeInitialRooms()
    {
        var rooms = new List<RoomInfo>(initialRooms.Values);
        initialRooms.Clear();
        return rooms;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        initialRooms.Clear();
        ConnectionFailed("Desconectado (" + cause + "). Podés reintentar.");
    }

    private void ConnectionFailed(string message)
    {
        openingRooms = false;
        SetStatus(message, true, true);
    }

    private void SetStatus(string message, bool canOpenRooms, bool canRetry)
    {
        connectionStatus.text = message;
        roomsButton.interactable = canOpenRooms;
        retryButton.interactable = canRetry;
    }
}
