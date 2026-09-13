using UnityEngine;
using TMPro;
using Photon.Realtime;

public class UI_RoomButtom : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _buttomText;

    public string roomName;

    public void Setup()
    {
        _buttomText.text = roomName;
        this.gameObject.SetActive(true);
    }


    public void JoinRoom()
    {
        PhotonManager.Instance.JoinRoom(roomName);
    }



}
