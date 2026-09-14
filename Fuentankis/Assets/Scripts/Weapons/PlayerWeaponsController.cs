using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponsController : MonoBehaviourPun
{
    [SerializeField] PlayerWeapon mainWeapon;
    [SerializeField] GrenadeLauncher grenadeLauncher;
    [SerializeField] Transform playerSprite;
    [SerializeField] float aimOffset;

    private PlayerCamera pC;

    private void Awake()
    {
        pC = GetComponent<PlayerCamera>();

        //mainWeapon.removeAmmoRPC = nameof(RemoveMainAmmo);
        //grenadeLauncher.removeAmmoRPC = nameof(RemoveGrenade);
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            CannonRotation(playerSprite, aimOffset);
        }
    }

    private void CannonRotation(Transform trns, float offst)
    {
        float posX = Mouse.current.position.x.ReadValue();
        float posY = Mouse.current.position.y.ReadValue();

        Vector3 mouse = new Vector3(posX, posY, 0f);

        Vector3 displacement = trns.position - pC.myCamera.ScreenToWorldPoint(mouse);

        float angle = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;

        trns.rotation = Quaternion.Euler(0f, 0f, angle + offst);


    }
}
