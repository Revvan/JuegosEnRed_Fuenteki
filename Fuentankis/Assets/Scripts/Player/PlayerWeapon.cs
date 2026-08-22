using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] GameObject bulletPrefab;

    private PhotonView myView;



    void Awake()
    {
        myView = GetComponent<PhotonView>();
        if (bulletPrefab == null){
            Debug.LogError("no bullet prefab bro...");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!myView.IsMine)
        { return; }

        ProcessShoot();
    }

    void ProcessShoot()
    {
        if (Keyboard.current != null){
            if (Keyboard.current.spaceKey.isPressed){
                PhotonNetwork.Instantiate(bulletPrefab.name, transform.position, Quaternion.identity);
            }
        }
    }
}
