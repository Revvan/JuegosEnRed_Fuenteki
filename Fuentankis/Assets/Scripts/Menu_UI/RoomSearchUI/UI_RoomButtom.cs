using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class UI_RoomButtom : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttomText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button joinButton;
    [SerializeField] private PhotonRoomManager manager;
    public string roomName;
    public void Setup(RoomInfo room)
    {
        roomName = room.Name;
        _buttomText.text = room.Name;
        countText.text = room.PlayerCount + "/" + (room.MaxPlayers == 0 ? "∞" : room.MaxPlayers.ToString());
        joinButton.interactable = room.IsOpen && (room.MaxPlayers == 0 || room.PlayerCount < room.MaxPlayers);
        gameObject.SetActive(true);
    }
    public void JoinRoom() { manager.JoinRoom(roomName); }
}
