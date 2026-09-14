using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldSystem : MonoBehaviourPun, IPunObservable
{
    private PhotonView myView;
    private PlayerWeapon myWeapon;

    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private Transform ShieldTransform;
    [SerializeField] private float radio = 2.0f;

    private Vector3 netShieldPosition;
    private Quaternion netShieldRotation;

    public bool isShieldActive = false;

    private bool netIsShieldActive = false;


    private void Awake()
    {
        myView = GetComponent<PhotonView>();
        myWeapon = GetComponent<PlayerWeapon>();
    }

    private void Start()
    {
        HideShield();
    }

    private void Update()
    {
        if (!myView.IsMine)
        {
            shieldPrefab.SetActive(netIsShieldActive);
        }
        else
        {
            ProcessShieldInput();
        }
    }

    private void FixedUpdate()
    {
        if (!myView.IsMine)
        {
            ShieldTransform.position = Vector3.Lerp(ShieldTransform.position, netShieldPosition, Time.deltaTime * 30);
            ShieldTransform.rotation = Quaternion.Lerp(ShieldTransform.rotation, netShieldRotation, Time.deltaTime * 30);
        }
        else
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPosition.z = transform.position.z;

            Vector3 direction = mouseWorldPosition - transform.position;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            direction.Normalize();

            ShieldTransform.position = transform.position + direction * radio;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            ShieldTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }

    private void ProcessShieldInput()
    {
        if (!myWeapon.HasAmmo() && isShieldActive || (Keyboard.current[myWeapon.KeyToShoot].isPressed))
        {
            HideShield();
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                if (myWeapon.HasAmmo())
                {
                    ShowShield();
                }
            }
        }
    }

    public void ShowShield()
    {
        isShieldActive = true;
        shieldPrefab.SetActive(true);
    }

    public void HideShield()
    {
        isShieldActive = false;
        shieldPrefab.SetActive(false);
    }

    public void ShieldTakeDamage()
    {
        HideShield();
        myView.RPC(nameof(myWeapon.RemoveMainAmmo), RpcTarget.All, 1);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ShieldTransform.position);
            stream.SendNext(ShieldTransform.rotation);
            stream.SendNext(isShieldActive);
        }
        else
        {
            netShieldPosition = (Vector3)stream.ReceiveNext();
            netShieldRotation = (Quaternion)stream.ReceiveNext();
            netIsShieldActive = (bool)stream.ReceiveNext();
        }
    }
}