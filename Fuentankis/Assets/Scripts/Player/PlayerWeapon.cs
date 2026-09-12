using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviourPun, IPunObservable
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform playerSprite;
    [SerializeField] Transform cannonTip;
    [SerializeField] float aimOffset;

    private PhotonView myView;
    private PlayerCamera pC;

    public int count = 0;


    public int ammoCount = 0;
    private int netAmmoCount = 0;

    [SerializeField] float fireRate = 0.5f;
    private float lastFireTime = 0;

    void Awake()
    {
        myView = GetComponent<PhotonView>();
        pC = GetComponent<PlayerCamera>();
        if (bulletPrefab == null) {
            Debug.LogError("no bullet prefab bro...");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            ammoCount = netAmmoCount;
        }
        else
        {
            ProcessShoot();
            CannonRotation(playerSprite, aimOffset);
        }
    }

    void ProcessShoot()
    {
        if (Keyboard.current != null) {
            if (Keyboard.current.spaceKey.isPressed) 
            {
                if (HasAmmo())
                {
                    if (lastFireTime + fireRate <= Time.time)
                    {
                        bulletController bullet = PhotonNetwork.Instantiate(bulletPrefab.name, cannonTip.position, cannonTip.rotation).GetComponent<bulletController>();
                        bullet.BulletImpulse(cannonTip);
                        if (bullet != null)
                        {
                            count++;
                        }
                        else
                        {
                            Debug.Log("Missing bullet instantiation");
                        }

                        myView.RPC("RemoveAmmo", RpcTarget.All, 1);

                        lastFireTime = Time.time;
                    }
                }
                else
                {
                    Debug.Log("No ammo left");
                }
            }
        }
    }

    private void CannonRotation(Transform trns, float offst)
    {
        float posX = Mouse.current.position.x.ReadValue();
        float posY = Mouse.current.position.y.ReadValue();

        Vector3 mouse = new Vector3(posX, posY, 0f);

        Vector3 displacement = trns.position - pC.myCamera.ScreenToWorldPoint(mouse);
        
        float angle = Mathf.Atan2 (displacement.y, displacement.x) * Mathf.Rad2Deg;

        trns.rotation = Quaternion.Euler(0f, 0f, angle + offst);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(cannonTip.position, 0.2f);
    //}


    public bool HasAmmo()
    {
        return ammoCount > 0;
    }

    [PunRPC]
    public void AddAmmo(int amount)
    {    
        ammoCount += amount;
        netAmmoCount += amount;
        //print("Here in add ammo");
    }

    [PunRPC]
    public void RemoveAmmo(int amount)
    {
        ammoCount -= amount;
        netAmmoCount -= amount;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ammoCount);
        }
        else
        {
            netAmmoCount = (int)stream.ReceiveNext();
        }
    }
}
