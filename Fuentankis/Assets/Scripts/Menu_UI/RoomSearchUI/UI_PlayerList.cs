using System;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerList : MonoBehaviour
{
    [Serializable] public class Row
    {
        public TMP_Text nameText, stateText;
        public Image avatar;
        public GameObject hostBadge;
    }
    [SerializeField] private TMP_Text title;
    [SerializeField] private Row[] rows;
    private readonly Color[] colors = { new Color32(40,195,255,255), new Color32(255,90,90,255),
        new Color32(72,236,104,255), new Color32(255,209,65,255) };

    public void Refresh()
    {
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        int maximum = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : rows.Length;
        title.text = "JUGADORES (" + players.Length + "/" + (maximum == 0 ? "∞" : maximum.ToString()) + ")";
        for (int i = 0; i < rows.Length; i++)
        {
            bool occupied = i < players.Length;
            var row = rows[i];
            row.nameText.transform.parent.gameObject.SetActive(maximum == 0 || i < maximum);
            row.nameText.text = occupied ? PhotonRoomManager.NameOf(players[i]) : "Esperando jugador...";
            row.nameText.color = occupied ? Color.white : new Color32(149,173,204,255);
            row.avatar.color = occupied ? colors[i % colors.Length] : new Color32(116,144,180,255);
            row.hostBadge.SetActive(occupied && players[i].IsMasterClient);
            row.stateText.text = !occupied ? "" : players[i].CustomProperties[PhotonRoomManager.PlayingKey] is bool playing && playing
                ? "Jugando" : "En sala";
        }
    }
}
