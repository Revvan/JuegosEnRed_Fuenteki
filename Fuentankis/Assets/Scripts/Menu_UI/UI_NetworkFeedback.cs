using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Three scene-authored rows; no runtime creation of UI objects.
public class UI_NetworkFeedback : MonoBehaviourPunCallbacks
{
    private struct Message { public string text; public Color color; public double expiresAt; }
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private TMP_Text[] lines;
    [SerializeField, Min(1)] private float messageSeconds = 6;
    private readonly List<Message> messages = new List<Message>(3);
    private readonly HashSet<int> playingPlayers = new HashSet<int>();
    private static readonly Color JoinedColor = new Color32(90, 238, 120, 255);
    private static readonly Color LeftColor = new Color32(255, 100, 100, 255);
    private Rect lastSafeArea;
    private Vector2 lastScreen;

    private void Start()
    {
        foreach (var player in PhotonNetwork.PlayerList)
            if (IsPlaying(player)) playingPlayers.Add(player.ActorNumber);
        if (PhotonNetwork.InRoom && IsPlaying(PhotonNetwork.LocalPlayer))
            Push(EntryMessage(PhotonNetwork.LocalPlayer), JoinedColor);
        else Refresh();
    }

    private static bool IsPlaying(Player player) =>
        player.CustomProperties[PhotonRoomManager.PlayingKey] is bool value && value;

    private static string EntryMessage(Player player) =>
        player.CustomProperties[PhotonRoomManager.ReturningKey] is bool returning && returning
            ? "El jugador " + PhotonRoomManager.NameOf(player) + " ha reingresado a la partida"
            : "Jugador " + PhotonRoomManager.NameOf(player) + " ingresó a la partida";

    public override void OnPlayerPropertiesUpdate(Player player, Hashtable changed)
    {
        if (!(changed[PhotonRoomManager.PlayingKey] is bool playing)) return;
        if (playing && playingPlayers.Add(player.ActorNumber))
            Push(EntryMessage(player), JoinedColor);
        else if (!playing && playingPlayers.Remove(player.ActorNumber))
            Push("Jugador " + PhotonRoomManager.NameOf(player) + " abandonó la partida", LeftColor);
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        if (IsPlaying(player) && playingPlayers.Add(player.ActorNumber))
            Push(EntryMessage(player), JoinedColor);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (playingPlayers.Remove(player.ActorNumber) || IsPlaying(player))
            Push("Jugador " + PhotonRoomManager.NameOf(player) + " abandonó la partida", LeftColor);
    }

    public override void OnMasterClientSwitched(Player player) =>
        Push("Jugador " + PhotonRoomManager.NameOf(player) + " es el nuevo host", new Color32(255, 217, 110, 255));

    public override void OnDisconnected(DisconnectCause cause)
    {
        messages.Clear();
        Push("Se perdió la conexión: " + cause, LeftColor, true);
    }

    private void Push(string text, Color color, bool persistent = false)
    {
        if (lines.Length == 0) return;
        while (messages.Count >= lines.Length) messages.RemoveAt(0);
        messages.Add(new Message { text = text, color = color,
            expiresAt = persistent ? double.PositiveInfinity : Time.unscaledTimeAsDouble + messageSeconds });
        Refresh();
    }

    private void Update()
    {
        var screen = new Vector2(Screen.width, Screen.height);
        var area = Screen.safeArea;
        if (screen.x > 0 && screen.y > 0 && (screen != lastScreen || area != lastSafeArea))
        {
            safeArea.anchorMin = area.min / screen; safeArea.anchorMax = area.max / screen;
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
            lastScreen = screen; lastSafeArea = area;
        }
        bool changed = messages.RemoveAll(message => Time.unscaledTimeAsDouble >= message.expiresAt) > 0;
        if (changed) Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].transform.parent.gameObject.SetActive(i < messages.Count);
            if (i >= messages.Count) continue;
            lines[i].text = messages[i].text; lines[i].color = messages[i].color;
        }
    }
}
