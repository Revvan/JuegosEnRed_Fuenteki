using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class RaceManager : MonoBehaviourPunCallbacks
{
    public static RaceManager Instance;
    private PhotonView myView;

    [SerializeField] float timeToAdd;
    [SerializeField] TextMeshProUGUI timer;
    DateTime UTCTime;
    public bool raceStarted;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        myView = GetComponent<PhotonView>();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UTCTime = DateTime.UtcNow.AddSeconds(timeToAdd);
            photonView.RPC("ReceiveStartTime", RpcTarget.AllBuffered, UTCTime.ToString());
        }
    }


    [PunRPC]
    public void ReceiveStartTime(string time)
    {
        UTCTime = DateTime.Parse(time);
        StartCoroutine(CheckUTCTime());
    }

    IEnumerator CheckUTCTime()
    {
        while (DateTime.UtcNow < UTCTime)
        {
            timer.text = (UTCTime - DateTime.UtcNow).ToString("ss\\.ff");
            yield return null;
        }
        timer.gameObject.SetActive(false);
        raceStarted = true;
        Debug.Log("empieza :OOO");
    }
}
