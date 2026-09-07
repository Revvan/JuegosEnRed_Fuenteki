using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{

    [SerializeField] private GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonManager.Instance.OnRoom += SpawnPlayer;
    }


    private void SpawnPlayer()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
    }

}
