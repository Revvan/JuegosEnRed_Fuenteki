using UnityEngine;
using Photon.Pun;

public class AmmoSpawn : MonoBehaviourPunCallbacks
{
    private PhotonView myView;

    [SerializeField] private GameObject ammo_prefab;
    [SerializeField] private GameObject grammo_prefab;
    [SerializeField] private GameObject spawn_container;
    [SerializeField] Vector3 worldSpace = Vector3.one;
    [SerializeField] Vector3 origin = Vector3.zero;
    [SerializeField] float grenadeThreshold = 0.5f;

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
            Debug.LogError("BulletAmmo prefab is not assigned in the inspector.");
        }
        if (grammo_prefab == null)
        {
            Debug.LogError("GrenadeAmmo prefab is not assigned in the inspector.");
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
        if (ammo_prefab != null && grammo_prefab != null)
        {
            string chosenPrefabName = "";

            if (Random.value > grenadeThreshold)
            {
                chosenPrefabName = ammo_prefab.name;
            }
            else
            {
                chosenPrefabName = grammo_prefab.name;
            }

            GameObject bullet = PhotonNetwork.InstantiateRoomObject(chosenPrefabName, 
                new Vector2(Random.value * (worldSpace.x) + origin.x, Random.value * (worldSpace.y) + origin.y), 
                Quaternion.identity);
        }
    }
}