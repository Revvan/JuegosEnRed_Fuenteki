using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Gameplay, scoring and player/arena resets will subscribe to this flow later.
public class GameManager : MonoBehaviourPunCallbacks
{
    public enum RoundState { WaitingForPlayers, Countdown, Playing, RoundEnd, Results }

    [Header("Player limits for new rooms")]
    [SerializeField, Range(1, 12)] private int minimumPlayers = 2;
    [SerializeField, Range(1, 12)] private int maximumPlayers = 12;
    private const string MinimumPlayersKey = "room.minimumPlayers";
    public int MinimumPlayers => Read(MinimumPlayersKey, minimumPlayers);
    public int MaximumPlayers => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : maximumPlayers;

    private void OnValidate()
    {
        maximumPlayers = Mathf.Clamp(maximumPlayers, 1, 12);
        minimumPlayers = Mathf.Clamp(minimumPlayers, 1, maximumPlayers);
    }

    public RoomOptions CreateRoomOptions() => new RoomOptions {
        MaxPlayers = (byte)maximumPlayers, PlayerTtl = 0, EmptyRoomTtl = 0,
        CustomRoomProperties = new Hashtable { { MinimumPlayersKey, minimumPlayers } }
    };

    [Header("Round timers")]
    [SerializeField, Min(1)] private float countdownSeconds = 3f;
    [SerializeField, Range(1, 120)] private float roundSeconds = 120f;
    [SerializeField, Min(0.1f)] private float roundEndSeconds = 1f;
    [SerializeField, Min(1)] private float resultsSeconds = 10f;

    private const string StateKey = "round.state";
    private const string NumberKey = "round.number";
    private const string DeadlineKey = "round.deadline";
    private bool awaitingUpdate;
    private float retryAt;
    private RoundState lastLoggedState;
    private int lastLoggedRound = -1;

    public RoundState State => (RoundState)Read(StateKey, (int)RoundState.WaitingForPlayers);
    public int RoundNumber => Read(NumberKey, 0);
    public double RemainingSeconds => System.Math.Max(0, Read(DeadlineKey, 0d) - PhotonNetwork.Time);
    public string Feedback { get; private set; } = "Conectando...";
    private void Start()
    {
        if (PhotonNetwork.InRoom) Feedback = "Conectado a la sala.";
    }

    public int PlayerCount
    {
        get
        {
            int count = 0;
            foreach (Player player in PhotonNetwork.PlayerList)
                if (!player.IsInactive) count++;
            return count;
        }
    }

    private static T Read<T>(string key, T fallback)
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties[key] is T value)
            return value;
        return fallback;
    }


    private void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        // Rooms created through the browser wait for the host's explicit start.
        if (PhotonNetwork.CurrentRoom.CustomProperties[PhotonRoomManager.StartedKey] is bool started && !started) return;

        if (lastLoggedRound != RoundNumber || lastLoggedState != State)
        {
            lastLoggedRound = RoundNumber;
            lastLoggedState = State;
            Debug.Log($"[F04] Round {RoundNumber}: {State}");
        }

        if (!PhotonNetwork.IsMasterClient || (awaitingUpdate && Time.unscaledTime < retryAt)) return;

        // Initialize once. Room properties survive the departure of the current Master.
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(StateKey))
        {
            Publish(RoundState.WaitingForPlayers, 0, 0, false);
            return;
        }

        switch (State)
        {
            case RoundState.WaitingForPlayers:
                if (PlayerCount >= MinimumPlayers)
                    Publish(RoundState.Countdown, countdownSeconds, RoundNumber + 1);
                break;
            case RoundState.Countdown:
                if (PlayerCount < MinimumPlayers)
                    Publish(RoundState.WaitingForPlayers, 0, RoundNumber);
                else if (RemainingSeconds <= 0)
                    Publish(RoundState.Playing, Mathf.Clamp(roundSeconds, 1, 120), RoundNumber);
                break;
            case RoundState.Playing:
                if (RemainingSeconds <= 0)
                    Publish(RoundState.RoundEnd, roundEndSeconds, RoundNumber);
                break;
            case RoundState.RoundEnd:
                if (RemainingSeconds <= 0)
                    Publish(RoundState.Results, resultsSeconds, RoundNumber);
                break;
            case RoundState.Results:
                if (RemainingSeconds <= 0)
                    Publish(RoundState.WaitingForPlayers, 0, RoundNumber);
                break;
        }
    }
    ///<summary>
    /// Publishes the next round state to the Room's Custom Properties.
    /// Uses an expected-value check to prevent delayed transitions from
    /// overwriting a newer state, and stores a shared Photon deadline
    /// so every client can calculate the same remaining time.
    /// </summary>
    private void Publish(RoundState nextState, double duration, int number, bool compare = true)
    {
        // Compare the snapshot we read, so a delayed transition cannot overwrite a newer one.
        var expected = compare ? new Hashtable {
            { StateKey, (int)State }, { NumberKey, RoundNumber }, { DeadlineKey, Read(DeadlineKey, 0d) }
        } : null;
        var values = new Hashtable {
            { StateKey, (int)nextState }, { NumberKey, number },
            { DeadlineKey, duration > 0 ? PhotonNetwork.Time + duration : 0d }
        };
        // Legacy rooms created without these options adopt the initializing Master's minimum.
        if (!compare && !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MinimumPlayersKey))
            values[MinimumPlayersKey] = minimumPlayers;
        awaitingUpdate = PhotonNetwork.CurrentRoom.SetCustomProperties(values, expected);
        retryAt = Time.unscaledTime + 2f;
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(StateKey)) awaitingUpdate = false;
    }

    public override void OnJoinedRoom()
    {
        awaitingUpdate = false;
        lastLoggedRound = -1;
        Feedback = "Conectado a la sala.";
    }

    public override void OnPlayerEnteredRoom(Player player)
    { Feedback = player.NickName + " se unió a la sala."; }

    public override void OnPlayerLeftRoom(Player player)
    { Feedback = player.NickName + " abandonó la sala."; }

    public override void OnMasterClientSwitched(Player player)
    {
        awaitingUpdate = false;
        Feedback = player.NickName + " es el nuevo Master. El reloj continúa.";
    }

    public override void OnLeftRoom()
    { awaitingUpdate = false; Feedback = "Fuera de la sala."; }

    public override void OnDisconnected(DisconnectCause cause)
    { awaitingUpdate = false; Feedback = "Desconectado: " + cause; }
}
