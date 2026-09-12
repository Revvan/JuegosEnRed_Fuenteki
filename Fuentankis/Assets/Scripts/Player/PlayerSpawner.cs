using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPun
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private List<Transform> spawnPoint;
    public int playerCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PhotonManager.Instance.OnRoom += PlayerJoinedRoom;
        if(spawnPoint.Count <=0){
            Debug.Log("no spawn point");
        }
    }


    private void SpawnPlayer()
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount % spawnPoint.Count;
        //Debug.Log("SpawnPlayer: "+ playerCount + " pos: " + spawnPoint[playerCount].name);
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint[playerCount].position, Quaternion.identity);

    }



    private void PlayerJoinedRoom(){
        SpawnPlayer();
    }

    public Transform RandomSpawnPoint()
    {
        return spawnPoint[Random.Range(0, spawnPoint.Count)];
    }

}
