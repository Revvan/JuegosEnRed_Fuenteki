using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform Player;
    [SerializeField] Transform cannonTip;
    [SerializeField] float aimOffset;

    private PhotonView myView;

    public int count = 0;

    [SerializeField] float fireRate = 0.5f;
    private float lastFireTime = 0;

    void Awake()
    {
        myView = GetComponent<PhotonView>();
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
        CannonRotation(Player, aimOffset);
    }

    void ProcessShoot()
    {
        if (Keyboard.current != null) {
            if (Keyboard.current.spaceKey.isPressed) {

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
                    lastFireTime = Time.time;
                }
            }
        }
    }

    private void CannonRotation(Transform trns, float offst)
    {
        float posX = Mouse.current.position.x.ReadValue();
        float posY = Mouse.current.position.y.ReadValue();

        Vector3 mouse = new Vector3(posX, posY, 0f);

        Vector3 displacement = trns.position - Camera.main.ScreenToWorldPoint(mouse);
        
        float angle = Mathf.Atan2 (displacement.y, displacement.x) * Mathf.Rad2Deg;

        trns.rotation = Quaternion.Euler(0f, 0f, angle + offst);
    }
}
