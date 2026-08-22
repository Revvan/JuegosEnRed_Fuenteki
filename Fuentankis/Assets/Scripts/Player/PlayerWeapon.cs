using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] GameObject bulletPrefab;

    private PhotonView myView;

    public int count = 0;

    [SerializeField] float fireRate = 0.5f;
    private float lastFireTime = 0;

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

                if (lastFireTime + fireRate <= Time.time)
                {
                    bulletController bullet = PhotonNetwork.Instantiate(bulletPrefab.name, transform.position, Quaternion.identity).GetComponent<bulletController>();
                    if (bullet != null)
                    {
                        count++;
                    }
                    else
                    {
                        Debug.Log("Missing bullet instantiation");
                    }
                    lastFireTime = Time.time;
                }
            }
        }
    }
}
