using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform playerSprite;
    [SerializeField] Transform cannonTip;
    [SerializeField] float aimOffset;
    [SerializeField] Key keyToShoot;

    private PhotonView myView;
    private PlayerCamera pC;

    public int count = 0;
    public int ammo = 0;

    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float shootingForce = 10f;
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
        if (!myView.IsMine)
        { return; }

        ProcessShoot();
        CannonRotation(playerSprite, aimOffset);
    }

    void ProcessShoot()
    {
        if (Keyboard.current != null && ammo > 0) {
            //Keyboard.current.spaceKey.isPressed
            if (Keyboard.current[keyToShoot].isPressed) {

                if (lastFireTime + fireRate <= Time.time)
                {
                    BulletBase bullet = PhotonNetwork.Instantiate(bulletPrefab.name, cannonTip.position, cannonTip.rotation).GetComponent<BulletBase>();
                    bullet.BulletImpulse(cannonTip, shootingForce);
                    if (bullet != null)
                    {
                        count++;
                        ammo--;
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

    public void ObtainAmmo(int amount)
    {
        ammo += amount;
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
}
