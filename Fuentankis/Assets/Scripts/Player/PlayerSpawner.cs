using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPun
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private List<Transform> spawnPoint;
    public int playerCount = 0;
    private bool spawned;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PhotonManager.Instance.OnRoom += PlayerJoinedRoom;
        if (PhotonNetwork.InRoom) PlayerJoinedRoom();
        if(spawnPoint.Count <=0){
            //Debug.Log("no spawn point");
        }
    }


    private void SpawnPlayer()
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount % spawnPoint.Count;
        var a = RandomSpawnPoint().position;
        Debug.Log("SpawnPlayer: "+ playerCount + " pos: " + a);
        PhotonNetwork.Instantiate(playerPrefab.name,a, Quaternion.identity);


    }



    private void PlayerJoinedRoom(){
        Debug.Log("join room event");   
        if (spawned) return;
        spawned = true;
        SpawnPlayer();
    }

    private void OnDestroy()
    {
        if (PhotonManager.Instance != null) PhotonManager.Instance.OnRoom -= PlayerJoinedRoom;
        if (Instance == this) Instance = null;
    }

    public Transform RandomSpawnPoint()
    {
        return spawnPoint[Random.Range(0, spawnPoint.Count)];
    }

}
