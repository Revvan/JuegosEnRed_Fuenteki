using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls serialized scene elements only. The Master publishes the final ranking.
public class UI_Results : MonoBehaviourPunCallbacks
{
    [Serializable] private class Row
    {
        public GameObject root;
        public TMP_Text position, playerName, kills, deaths;
        public Image medal, avatar, background;
    }
    [Serializable] private class PodiumPlace
    {
        public GameObject root;
        public TMP_Text playerName;
        public Image avatar;
    }
    [SerializeField] private GameManager manager;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text winnerName, winnerKills, winnerDeaths, statusText, countdownText;
    [SerializeField] private Image winnerAvatar;
    [SerializeField] private Row[] rows;
    [SerializeField] private PodiumPlace[] podium;
    [SerializeField] private Button roomsButton, menuButton;
    [SerializeField] private ScrollRect rankingScroll;
    private bool leaving, loading, connectionLost;
    private string destination;
    private int displayedRound = -1;
    private readonly Color[] colors = {
        new Color32(55, 198, 255, 255), new Color32(255, 92, 92, 255),
        new Color32(80, 237, 102, 255), new Color32(255, 211, 65, 255)
    };

    private void Awake() => panel.SetActive(false);
    private void Update()
    {
        bool show = PhotonNetwork.InRoom &&
            (manager.State == GameManager.RoundState.RoundEnd || manager.State == GameManager.RoundState.Results);
        panel.SetActive(show || leaving || connectionLost);
        if (connectionLost) return;
        if (!show || leaving) return;
        bool ready = manager.State == GameManager.RoundState.Results;
        roomsButton.interactable = menuButton.interactable = ready;
        if (!ready)
        {
            statusText.text = "Fin de ronda. Sincronizando resultados...";
            countdownText.text = "Preparando ranking...";
            winnerName.text = "Calculando..."; winnerKills.text = "Kills: —"; winnerDeaths.text = "Deaths: —";
            foreach (var row in rows) row.root.SetActive(false);
            foreach (var place in podium) place.root.SetActive(false);
            return;
        }
        if (displayedRound != manager.RoundNumber)
        {
            var board = manager.Results;
            if (board.round != manager.RoundNumber) return;
            ShowScores(board);
            displayedRound = board.round;
        }
        countdownText.text = $"Volviendo a salas en {Mathf.CeilToInt((float)manager.RemainingSeconds)} s";
        if (manager.RemainingSeconds <= 0) ReturnToRooms();
    }

    public void ShowScores(GameManager.Scoreboard board)
    {
        var entries = board.rows ?? Array.Empty<GameManager.ScoreEntry>();
        statusText.text = "Resultado final sincronizado.";
        winnerName.text = entries.Length > 0 ? entries[0].name : "Sin participantes";
        winnerKills.text = "Kills: " + (entries.Length > 0 ? entries[0].kills : 0);
        winnerDeaths.text = "Deaths: " + (entries.Length > 0 ? entries[0].deaths : 0);
        if (entries.Length > 0) winnerAvatar.color = ColorFor(entries[0].actor);
        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i]; row.root.SetActive(i < entries.Length);
            if (i >= entries.Length) continue;
            var entry = entries[i];
            row.position.text = (i + 1).ToString(); row.playerName.text = entry.name;
            row.kills.text = entry.kills.ToString(); row.deaths.text = entry.deaths.ToString();
            row.avatar.color = ColorFor(entry.actor);
            row.medal.color = i == 0 ? new Color32(255, 208, 58, 255) :
                i == 1 ? new Color32(189, 215, 245, 255) : i == 2 ? new Color32(226, 151, 87, 255) : Color.white;
            row.background.color = i == 0 ? new Color32(18, 45, 87, 255) : new Color32(28, 66, 121, 255);
        }
        for (int i = 0; i < podium.Length; i++)
        {
            podium[i].root.SetActive(i < entries.Length);
            if (i >= entries.Length) continue;
            podium[i].playerName.text = entries[i].name;
            podium[i].avatar.color = ColorFor(entries[i].actor);
        }
        if (rankingScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            var size = rankingScroll.content.sizeDelta;
            size.y = Mathf.Max(rankingScroll.viewport.rect.height, Mathf.Min(entries.Length, rows.Length) * 54);
            rankingScroll.content.sizeDelta = size;
            rankingScroll.verticalNormalizedPosition = 1;
        }
    }

    private Color ColorFor(int actor) => colors[Mathf.Abs(actor - 1) % colors.Length];
    public void ReturnToRooms() => Leave("RoomsScene");
    public void ReturnToMenu() => Leave("UI");
    private void Leave(string scene)
    {
        if (leaving || loading) return;
        leaving = true; destination = scene;
        roomsButton.interactable = menuButton.interactable = false;
        statusText.text = "Saliendo de la sala...";
        if (PhotonManager.Instance != null) PhotonManager.Instance.LeavingGameplay = true;
        if (scene == "UI") { PhotonNetwork.Disconnect(); if (!PhotonNetwork.IsConnected) LoadDestination(); }
        else if (PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.LeaveRoom(false))
            { leaving = false; statusText.text = "No se pudo salir. Intentá nuevamente."; }
        }
        else if (PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.IsConnected) LoadDestination();
    }

    public override void OnConnectedToMaster() { if (leaving) LoadDestination(); }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (leaving) LoadDestination();
        else
        {
            connectionLost = true; panel.SetActive(true);
            statusText.text = "Desconectado: " + cause;
            countdownText.text = "Regresá a salas para reconectar.";
            roomsButton.interactable = menuButton.interactable = true;
        }
    }
    private void LoadDestination()
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadSceneAsync(destination);
    }
}
