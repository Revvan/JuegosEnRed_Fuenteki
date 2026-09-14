using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviourPun
{
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] Transform cannonTip;
    [SerializeField] Key keyToShoot;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] string removeAmmoRPC = "";
    public int count = 0;
    public int ammoCount = 0;

    private PhotonView myView;

    protected int netAmmoCount = 0;

    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float shootingForce = 10f;
    private float lastFireTime = 0;

    void Awake()
    {
        myView = GetComponent<PhotonView>();
        Debug.Log(myView);
        if (bulletPrefab == null)
        {
            Debug.LogError("no bullet prefab bro...");
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            ammoCount = netAmmoCount;
        }
        else
        {
            ProcessShoot();
        }
        text.text = ammoCount.ToString();
    }

    void ProcessShoot()
    {
        if (Keyboard.current != null)
        {
            //Keyboard.current.spaceKey.isPressed
            if (keyToShoot != Key.None && Keyboard.current[keyToShoot].isPressed)
            {

                if (HasAmmo())
                {
                    if (lastFireTime + fireRate <= Time.time)
                    {
                        BulletBase bullet = PhotonNetwork.Instantiate(bulletPrefab.name, cannonTip.position, cannonTip.rotation).GetComponent<BulletBase>();
                        bullet.BulletImpulse(cannonTip, shootingForce);
                        if (bullet != null)
                        {
                            count++;
                        }
                        else
                        {
                            Debug.Log("Missing bullet instantiation");
                        }

                        myView.RPC(removeAmmoRPC, RpcTarget.All, 1);

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

    public bool HasAmmo()
    {
        return ammoCount > 0;
    }

    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        netAmmoCount += amount;
        //print("Here in add ammo");
    }

    public void RemoveAmmo(int amount)
    {
        ammoCount -= amount;
        netAmmoCount -= amount;
    }
}
