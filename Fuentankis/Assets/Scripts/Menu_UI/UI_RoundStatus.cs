using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Updates existing scene labels. Does not create UI or control the round.
public class UI_RoundStatus : MonoBehaviour
{
    [SerializeField] private GameManager manager;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Image panelBackground;

    private void Update()
    {
        bool playing = PhotonNetwork.InRoom && manager.State == GameManager.RoundState.Playing;
        panelBackground.enabled = !playing;
        feedbackText.gameObject.SetActive(!playing);
        feedbackText.text = manager.Feedback;
        if (!PhotonNetwork.InRoom)
        {
            statusText.text = "Esperando conexión a una sala...";
            return;
        }

        int seconds = Mathf.CeilToInt((float)manager.RemainingSeconds);
        switch (manager.State)
        {
            case GameManager.RoundState.WaitingForPlayers:
                statusText.text = $"Esperando jugadores: {manager.PlayerCount}/{manager.MinimumPlayers} mínimo";
                break;
            case GameManager.RoundState.Countdown:
                statusText.text = $"Ronda {manager.RoundNumber} comienza en {seconds}";
                break;
            case GameManager.RoundState.Playing:
                statusText.text = $"Ronda {manager.RoundNumber}   •   {seconds / 60:00}:{seconds % 60:00}";
                break;
            case GameManager.RoundState.RoundEnd:
                statusText.text = "Tiempo de ronda terminado";
                break;
            case GameManager.RoundState.Results:
                statusText.text = $"Resultados • Volviendo a salas en {seconds} s";
                break;
        }
    }
}
