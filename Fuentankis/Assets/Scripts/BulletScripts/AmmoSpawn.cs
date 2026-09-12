using UnityEngine;
using Photon.Pun;

public class AmmoSpawn : MonoBehaviourPunCallbacks
{
    private PhotonView myView;

    [SerializeField] private GameObject ammo_prefab;
    [SerializeField] private GameObject spawn_container;
    [SerializeField] private Vector2 spawn_position = new Vector2(1.0f, 1.0f);

    [SerializeField] private float spawnTime = 5.0f;
    private float spawnStartTime = 0.0f;

    private void Awake()
    {
        spawnStartTime = Time.time;
        myView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (ammo_prefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned in the inspector.");
        }
        if (spawn_container == null)
        {
            spawn_container = this.gameObject;
        }
        PhotonManager.Instance.OnRoom += SpawnAmmo;
    }

    private void Update()
    {
        if (spawnStartTime + spawnTime <= Time.time)
        {
            SpawnAmmo();
            spawnStartTime = Time.time;
        }
    }

    private void SpawnAmmo()
    {
        if (ammo_prefab != null)
        {
            GameObject bullet = PhotonNetwork.InstantiateRoomObject(ammo_prefab.name, 
                new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)), 
                Quaternion.identity);
        }
    }
}