using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Room flow and score snapshots. Each victim owns its death record in room properties.
public class GameManager : MonoBehaviourPunCallbacks
{
    public enum RoundState { WaitingForPlayers, Countdown, Playing, RoundEnd, Results }

    [Header("Player limits for new rooms")]
    [SerializeField, Range(1, 12)] private int minimumPlayers = 2;
    [SerializeField, Range(1, 12)] private int maximumPlayers = 12;
    private const string MinimumPlayersKey = "room.minimumPlayers";
    public const string DurationKey = "room.duration";
    private const string ScorePrefix = "score.";
    private const string ResultsKey = "round.results";
    public int RoundDuration => Mathf.Clamp(Read(DurationKey, 120), 60, 300);
    public bool GameplayActive => PhotonNetwork.InRoom && State == RoundState.Playing && RemainingSeconds > 0;
    [Serializable] public class ScoreEntry { public int actor, kills, deaths, tie; public string id, name; }
    [Serializable] public class Scoreboard { public int round; public ScoreEntry[] rows = Array.Empty<ScoreEntry>(); }
    [Serializable] private class Credit { public int actor; public string id, name; }
    [Serializable] private class DeathRecord
    {
        public int round, actor;
        public string id, name;
        public List<Credit> credits = new List<Credit>();
    }
    private DeathRecord localRecord;
    public bool HasParticipated(string id)
    {
        string stored = Read(ScorePrefix + id, "");
        if (string.IsNullOrEmpty(stored)) return false;
        var record = JsonUtility.FromJson<DeathRecord>(stored);
        return record != null && record.round == RoundNumber;
    }
    private int? requestedDuration;
    public Scoreboard Results => JsonUtility.FromJson<Scoreboard>(Read(ResultsKey, "{}")) ?? new Scoreboard();

    public override void OnEnable() { base.OnEnable(); LifeComponent.PlayerKilled += RecordDeath; }
    public override void OnDisable() { LifeComponent.PlayerKilled -= RecordDeath; base.OnDisable(); }

    // Writes only this client's record, so simultaneous deaths never overwrite each other.
    private void RegisterLocalPlayer()
    {
        if (SceneManager.GetActiveScene().name != "MainGame" || RoundNumber <= 0 ||
            (State != RoundState.Countdown && State != RoundState.Playing)) return;
        if (localRecord != null && localRecord.round == RoundNumber) return;
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;
        string id = PlayerIdentity.LocalId;
        string stored = Read(ScorePrefix + id, "");
        localRecord = string.IsNullOrEmpty(stored) ? null : JsonUtility.FromJson<DeathRecord>(stored);
        if (localRecord == null || localRecord.round != RoundNumber)
            localRecord = new DeathRecord { round = RoundNumber, actor = actor, id = id,
                name = PhotonRoomManager.NameOf(PhotonNetwork.LocalPlayer) };
        localRecord.name = PhotonRoomManager.NameOf(PhotonNetwork.LocalPlayer);
        SaveLocalRecord();
    }

    private void SaveLocalRecord() => PhotonNetwork.CurrentRoom.SetCustomProperties(
        new Hashtable {
            { ScorePrefix + localRecord.id, JsonUtility.ToJson(localRecord) },
            // Keep old connection aliases after departure, including for projectiles still in flight.
            { "identity." + PhotonNetwork.LocalPlayer.ActorNumber, localRecord.id }
        });

    private static string IdentityFor(int actor)
    {
        string stored = Read("identity." + actor, "");
        if (!string.IsNullOrEmpty(stored)) return stored;
        return PhotonNetwork.CurrentRoom.GetPlayer(actor)?.UserId ?? "";
    }

    private void RecordDeath(int attacker, int victim, string attackerName, string victimName, int cause)
    {
        if (!GameplayActive || victim != PhotonNetwork.LocalPlayer.ActorNumber) return;
        RegisterLocalPlayer();
        if (localRecord == null) return;
        localRecord.name = victimName;
        localRecord.credits.Add(new Credit { actor = attacker, id = IdentityFor(attacker), name = attackerName });
        SaveLocalRecord();
    }

    public Scoreboard CalculateScores()
    {
        var players = new Dictionary<string, ScoreEntry>();
        ScoreEntry Get(string id, int actor, string name)
        {
            if (string.IsNullOrEmpty(id)) id = "actor:" + actor; // Legacy records never merge by nickname.
            if (!players.TryGetValue(id, out var entry))
                players.Add(id, entry = new ScoreEntry { id = id, actor = actor, name = name });
            entry.actor = Mathf.Min(entry.actor, actor); // Keep the original display color on reconnect.
            return entry;
        }
        if (!PhotonNetwork.InRoom) return new Scoreboard();
        foreach (var pair in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            if (!(pair.Key is string key) || !key.StartsWith(ScorePrefix) || !(pair.Value is string json)) continue;
            var record = JsonUtility.FromJson<DeathRecord>(json);
            if (record == null || record.round != RoundNumber) continue;
            var victim = Get(record.id, record.actor, record.name);
            victim.name = record.name;
            victim.deaths += record.credits.Count;
            foreach (var credit in record.credits)
                if (credit.actor > 0 && (string.IsNullOrEmpty(credit.id)
                    ? credit.actor != record.actor : credit.id != record.id))
                    Get(credit.id, credit.actor, credit.name).kills++;
        }
        var rows = new List<ScoreEntry>(players.Values);
        foreach (var entry in rows) entry.tie = UnityEngine.Random.Range(0, int.MaxValue);
        rows.Sort((a, b) => a.kills != b.kills ? b.kills.CompareTo(a.kills) :
            a.deaths != b.deaths ? a.deaths.CompareTo(b.deaths) :
            a.tie != b.tie ? a.tie.CompareTo(b.tie) : a.actor.CompareTo(b.actor));
        return new Scoreboard { round = RoundNumber, rows = rows.ToArray() };
    }

    public void ChangeDuration(int direction)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient ||
            !(PhotonNetwork.CurrentRoom.CustomProperties[PhotonRoomManager.StartedKey] is bool started) || started) return;
        int next = Mathf.Clamp((requestedDuration ?? RoundDuration) + Math.Sign(direction) * 15, 60, 300);
        if (PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { DurationKey, next } },
            new Hashtable { { PhotonRoomManager.StartedKey, false } })) requestedDuration = next;
    }
    public int MinimumPlayers => Read(MinimumPlayersKey, minimumPlayers);
    public int MaximumPlayers => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : maximumPlayers;

    private void OnValidate()
    {
        maximumPlayers = Mathf.Clamp(maximumPlayers, 1, 12);
        minimumPlayers = Mathf.Clamp(minimumPlayers, 1, maximumPlayers);
    }

    public RoomOptions CreateRoomOptions() => new RoomOptions {
        MaxPlayers = (byte)maximumPlayers, PlayerTtl = 0, EmptyRoomTtl = 0, PublishUserId = true,
        CustomRoomProperties = new Hashtable { { MinimumPlayersKey, minimumPlayers }, { DurationKey, 120 } }
    };

    [Header("Round timers")]
    [SerializeField, Min(1)] private float countdownSeconds = 3f;
    [SerializeField, Min(0.1f)] private float roundEndSeconds = 2f;
    [SerializeField, Min(1)] private float resultsSeconds = 20f;

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
        RegisterLocalPlayer();
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
                    Publish(RoundState.Playing, RoundDuration, RoundNumber);
                break;
            case RoundState.Playing:
                if (RemainingSeconds <= 0)
                    Publish(RoundState.RoundEnd, roundEndSeconds, RoundNumber);
                break;
            case RoundState.RoundEnd:
                if (PhotonNetwork.CurrentRoom.IsOpen) PhotonNetwork.CurrentRoom.IsOpen = false;
                if (PhotonNetwork.CurrentRoom.IsVisible) PhotonNetwork.CurrentRoom.IsVisible = false;
                if (RemainingSeconds <= 0)
                    Publish(RoundState.Results, resultsSeconds, RoundNumber);
                break;
            case RoundState.Results:
                // Each gameplay client leaves when the shared results deadline expires.
                if (PhotonNetwork.CurrentRoom.IsOpen) PhotonNetwork.CurrentRoom.IsOpen = false;
                if (PhotonNetwork.CurrentRoom.IsVisible) PhotonNetwork.CurrentRoom.IsVisible = false;
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
        if (nextState == RoundState.Results)
            values[ResultsKey] = JsonUtility.ToJson(CalculateScores());
        // Legacy rooms created without these options adopt the initializing Master's minimum.
        if (!compare && !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MinimumPlayersKey))
            values[MinimumPlayersKey] = minimumPlayers;
        awaitingUpdate = PhotonNetwork.CurrentRoom.SetCustomProperties(values, expected);
        retryAt = Time.unscaledTime + 2f;
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(StateKey)) awaitingUpdate = false;
        if (changed[DurationKey] is int duration && requestedDuration == duration) requestedDuration = null;
    }

    public override void OnJoinedRoom()
    {
        awaitingUpdate = false;
        lastLoggedRound = -1;
        localRecord = null;
        requestedDuration = null;
        Feedback = "Conectado a la sala.";
    }

    public override void OnPlayerEnteredRoom(Player player)
    { Feedback = player.NickName + " se unió a la sala."; }

    public override void OnPlayerLeftRoom(Player player)
    { Feedback = player.NickName + " abandonó la sala."; }

    public override void OnMasterClientSwitched(Player player)
    {
        awaitingUpdate = false;
        requestedDuration = null;
        Feedback = player.NickName + " es el nuevo Master. El reloj continúa.";
    }

    public override void OnLeftRoom()
    { awaitingUpdate = false; Feedback = "Fuera de la sala."; }

    public override void OnDisconnected(DisconnectCause cause)
    { awaitingUpdate = false; Feedback = "Desconectado: " + cause; }
}
