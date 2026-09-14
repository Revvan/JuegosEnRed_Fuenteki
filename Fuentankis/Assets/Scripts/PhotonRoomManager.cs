using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Room navigation and chat. Every visual control is assigned in RoomsScene.
public class PhotonRoomManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public const string StartedKey = "room.started";
    public const string PlayingKey = "room.playing";
    private const byte ChatEvent = 41;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text minimumPlayersText, maximumPlayersText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Button decreaseDurationButton, increaseDurationButton;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject browserPanel, enteredPanel;
    [SerializeField] private TMP_Text localPlayerText, roomTitle, actionText, chatText;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private ScrollRect chatScroll;
    [SerializeField] private Button actionButton, createButton, sendButton;
    [SerializeField] private UI_PlayerList playerList;
    [SerializeField] private PhotonRoomSearcher searcher;
    private readonly Queue<string> messages = new Queue<string>();
    private bool busy, loading, waitingForStart, startRequested, announcedStart;
    private float nextSend, startRetryAt;

    public bool Busy => busy || loading;
    public bool HasStarted => PhotonNetwork.InRoom &&
        (PhotonNetwork.CurrentRoom.CustomProperties[StartedKey] is bool started ? started :
        PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("round.state"));

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false; // Late joiners explicitly choose Unirse.
        localPlayerText.text = "Jugador: " + PlayerIdentity.LoadName();
        statusText.text = PhotonNetwork.InLobby ? "Conectado. Elegí o creá una sala." : "Conectando...";
        if (PhotonNetwork.InRoom) OnJoinedRoom();
    }

    private void Update()
    {
        bool inRoom = PhotonNetwork.InRoom;
        browserPanel.SetActive(!inRoom);
        enteredPanel.SetActive(inRoom);
        createButton.interactable = PhotonNetwork.InLobby && !Busy;
        if (!inRoom) return;
        int duration = gameManager.RoundDuration;
        durationText.text = $"Tiempo de ronda\n{duration / 60:00}:{duration % 60:00}";
        bool canConfigure = PhotonNetwork.IsMasterClient && !HasStarted && !Busy && !startRequested;
        decreaseDurationButton.interactable = canConfigure && duration > 60;
        increaseDurationButton.interactable = canConfigure && duration < 300;
        if (gameManager.State == GameManager.RoundState.Results && gameManager.RemainingSeconds <= 0 && !Busy)
        { Back(); return; }
        roomTitle.text = "SALA: " + PhotonNetwork.CurrentRoom.Name;
        minimumPlayersText.text = "Jugadores mínimos\n" + gameManager.MinimumPlayers;
        maximumPlayersText.text = "Máximo de jugadores\n" + (gameManager.MaximumPlayers == 0 ? "Sin límite" : gameManager.MaximumPlayers.ToString());
        if (startRequested && Time.unscaledTime >= startRetryAt) startRequested = false;
        actionText.text = HasStarted ? "UNIRSE" : PhotonNetwork.IsMasterClient ? "INICIAR PARTIDA" : "ESPERANDO INICIAR";
        actionButton.interactable = !Busy && !startRequested && (HasStarted ||
            (PhotonNetwork.IsMasterClient && gameManager.PlayerCount >= gameManager.MinimumPlayers));
        sendButton.interactable = !Busy && !string.IsNullOrWhiteSpace(chatInput.text);
        if (HasStarted && !announcedStart)
        { announcedStart = true; AddMessage("Partida iniciada..."); }
    }

    public void CreateRoom()
    {
        if (Busy || !PhotonNetwork.InLobby) return;
        string name = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) { statusText.text = "Escribí un nombre de sala."; return; }
        var options = gameManager.CreateRoomOptions();
        options.CustomRoomProperties[StartedKey] = false;
        busy = PhotonNetwork.CreateRoom(name, options);
        statusText.text = busy ? "Creando sala..." : "No se pudo enviar la solicitud.";
    }

    public void IncreaseDuration() { if (!Busy && !startRequested) gameManager.ChangeDuration(1); }
    public void DecreaseDuration() { if (!Busy && !startRequested) gameManager.ChangeDuration(-1); }

    public void JoinRoom(string name)
    {
        if (Busy || !PhotonNetwork.InLobby) return;
        busy = PhotonNetwork.JoinRoom(name);
        statusText.text = busy ? "Entrando a " + name + "..." : "No se pudo solicitar el ingreso.";
    }

    public void StartOrJoin()
    {
        if (Busy || !PhotonNetwork.InRoom) return;
        if (HasStarted) { EnterGameplay(); return; }
        if (!PhotonNetwork.IsMasterClient || gameManager.PlayerCount < gameManager.MinimumPlayers || startRequested) return;
        startRequested = PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable { { StartedKey, true } }, new Hashtable { { StartedKey, false } });
        startRetryAt = Time.unscaledTime + 3;
    }

    private void EnterGameplay()
    {
        if (loading || busy || !PhotonNetwork.InRoom) return;
        loading = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { PlayingKey, true } });
        SceneManager.LoadSceneAsync("MainGame");
    }

    public void SendChat()
    {
        if (!PhotonNetwork.InRoom || Busy || Time.unscaledTime < nextSend) return;
        string message = chatInput.text.Trim();
        if (string.IsNullOrEmpty(message)) return;
        if (message.Length > 160) message = message.Substring(0, 160);
        message = message.Replace('\n', ' ').Replace('\r', ' ');
        if (!PhotonNetwork.RaiseEvent(ChatEvent, message,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable)) return;
        AddMessage(NameOf(PhotonNetwork.LocalPlayer) + ": " + message);
        chatInput.SetTextWithoutNotify(""); chatInput.ActivateInputField();
        nextSend = Time.unscaledTime + 0.5f;
    }

    public void OnEvent(EventData data)
    {
        if (data.Code != ChatEvent || !PhotonNetwork.InRoom || !(data.CustomData is string message)) return;
        var sender = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
        if (sender == null || message.Length > 160) return;
        AddMessage(NameOf(sender) + ": " + message.Replace('\n', ' ').Replace('\r', ' '));
    }

    private void AddMessage(string message)
    {
        messages.Enqueue(message);
        while (messages.Count > 30) messages.Dequeue();
        chatText.text = string.Join("\n", messages);
        Canvas.ForceUpdateCanvases();
        chatScroll.verticalNormalizedPosition = 0;
    }

    public static string NameOf(Player player) => string.IsNullOrWhiteSpace(player.NickName)
        ? "Jugador" + player.ActorNumber : player.NickName;

    public void Back()
    {
        if (Busy) return;
        if (PhotonNetwork.InRoom)
        {
            busy = PhotonNetwork.LeaveRoom(false);
            if (busy) AddMessage("Saliendo de la sala...");
        }
        else
        {
            loading = true;
            PhotonNetwork.Disconnect();
            SceneManager.LoadSceneAsync("UI");
        }
    }

    public void RefreshOrRetry()
    {
        if (Busy || PhotonNetwork.InRoom) return;
        if (!PhotonNetwork.IsConnected)
        { statusText.text = "Conectando..."; PhotonNetwork.ConnectUsingSettings(); }
        else if (PhotonNetwork.InLobby)
        { searcher.FetchRooms(); statusText.text = "Conectado. Lista actualizada."; }
        else statusText.text = "Conectando al lobby...";
    }

    public override void OnJoinedLobby()
    { busy = false; statusText.text = "Conectado. Elegí o creá una sala."; }
    public override void OnJoinedRoom()
    {
        busy = false; startRequested = false; announcedStart = false;
        waitingForStart = !HasStarted;
        messages.Clear(); chatInput.SetTextWithoutNotify("");
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { PlayingKey, false } });
        AddMessage("Te uniste a " + PhotonNetwork.CurrentRoom.Name + ".");
        if (HasStarted) { announcedStart = true; AddMessage("Partida iniciada..."); }
        playerList.Refresh();
    }
    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(StartedKey) && HasStarted && waitingForStart) EnterGameplay();
    }
    public override void OnPlayerEnteredRoom(Player player)
    { AddMessage(NameOf(player) + " se unió a la sala."); playerList.Refresh(); }
    public override void OnPlayerLeftRoom(Player player)
    { AddMessage(NameOf(player) + " abandonó la sala."); playerList.Refresh(); }
    public override void OnPlayerPropertiesUpdate(Player player, Hashtable changed) { playerList.Refresh(); }
    public override void OnMasterClientSwitched(Player player)
    { startRequested = false; AddMessage(NameOf(player) + " es el nuevo host."); playerList.Refresh(); }
    public override void OnLeftRoom()
    { waitingForStart = false; statusText.text = "Volviendo al lobby..."; }
    public override void OnCreateRoomFailed(short code, string message)
    {
        busy = false;
        statusText.text = code == ErrorCode.GameIdAlreadyExists ? "Ese nombre ya existe. Elegí otro." : "No se pudo crear: " + message;
    }
    public override void OnJoinRoomFailed(short code, string message)
    { busy = false; statusText.text = "No se pudo entrar: " + message; }
    public override void OnDisconnected(DisconnectCause cause)
    { busy = false; waitingForStart = false; statusText.text = "Desconectado: " + cause + ". Pulsá REINTENTAR."; }
}
