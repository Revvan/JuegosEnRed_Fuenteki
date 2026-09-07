using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPun
{

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private List<Transform> spawnPoint;
    public int playerCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonManager.Instance.OnRoom += PlayerJoinedRoom;
        if(spawnPoint.Count <=0){
            Debug.Log("no spawn point");
        }
    }


    private void SpawnPlayer()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint[playerCount].position, Quaternion.identity);
        playerCount++;
        if(playerCount > spawnPoint.Count){
            Debug.Log("spawn point overflow");
            playerCount = 0;
        }
    }

    [PunRPC]
    public void RPC_SpawnPlayer(){
        SpawnPlayer();
    }

    private void PlayerJoinedRoom(){
        photonView.RPC("RPC_SpawnPlayer", RpcTarget.AllBuffered);
    }

}
