using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShieldSystem : MonoBehaviourPun, IPunObservable
{
    private PhotonView myView;
    private PlayerWeapon myWeapon;

    [Header("Shield")]
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private Transform shieldContainer;
    [SerializeField] private Transform ShieldTransform;
    [SerializeField] private float radio = 2.0f;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image bar;

    [SerializeField] private float arcAngle = 120f;

    public bool isShieldActive;

    private float shieldAngle;
    private int shieldSegmentCount;

    private int netShieldSegmentCount;
    private float netShieldAngle;
    private bool netIsShieldActive;
    private int netShieldMask;

    private readonly List<ShieldObject> shieldObjects = new();
    private int shieldMask = 0;


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
            UpdateNetworkShield();
            return;
        }

        ProcessShieldInput();

        //if (isShieldActive)
        //{
        //    UpdateShieldAmount();
        //}
    }


    private void FixedUpdate()
    {
        if (!myView.IsMine)
        {
            shieldAngle = netShieldAngle;

            UpdateShieldTransform();
            return;
        }

        CalculateShieldRotation();
    }


    private void ProcessShieldInput()
    {
        if ((!myWeapon.HasAmmo() && isShieldActive) || Keyboard.current[myWeapon.KeyToShoot].isPressed)
        {
            HideShield();
        }

        if (Mouse.current != null && Mouse.current.rightButton.isPressed && myWeapon.HasAmmo())
        {
            ShowShield();
        }
    }

    public void ShowShield()
    {
        if (isShieldActive)
        {
            return;
        }
            
        isShieldActive = true;
        shieldMask = 0;
        text.text = "Active";
        bar.fillAmount = 1f;
        RebuildShield(myWeapon.ammoCount);
    }

    private void UpdateShieldAmount()
    {
        if (!isShieldActive)
            return;

        if (shieldSegmentCount == myWeapon.ammoCount)
            return;

        RebuildShield(myWeapon.ammoCount);
    }

    public void HideShield()
    {
        isShieldActive = false;
        text.text = "Inactive";
        bar.fillAmount = 0f;
        ClearShield();
    }

    private void ClearShield()
    {
        foreach (ShieldObject shield in shieldObjects)
        {
            if (shield != null)
            {
                Destroy(shield.gameObject);
            }
        }

        shieldObjects.Clear();
    }

    private void DestroyShieldSegment(int index)
    {
        if (index < 0 || index >= shieldObjects.Count)
        {
            return;
        }

        if (IsSegmentDestroyed(index))
        {
            return;
        }

        SetSegmentDestroyed(index);

        shieldObjects[index].gameObject.SetActive(false);
    }

    private void RebuildShield(int shieldSegments)
    {
        ClearShield();

        shieldSegmentCount = shieldSegments; //myWeapon.ammoCount

        if (shieldSegmentCount <= 0)
        {
            return;
        }

        for (int i = 0; i < shieldSegmentCount; i++)
        {
            GameObject shieldObject = Instantiate(shieldPrefab, shieldContainer); // PhotonNetwork.Instantiate(shieldPrefab.name, shieldContainer.position, Quaternion.identity); 

            ShieldObject shield = shieldObject.GetComponent<ShieldObject>();
            //shield.gameObject.transform.SetParent(shieldContainer);

            shield.Initialize(this, i);

            shieldObjects.Add(shield);
        }

        UpdateShieldTransform();
    }

    private void CalculateShieldRotation()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = transform.position.z;

        Vector3 direction = mouseWorldPosition - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        shieldAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        UpdateShieldTransform();
    }

    private void UpdateShieldTransform()
    {
        int segmentCount = shieldObjects.Count;

        if (segmentCount == 0)
        {
            return;
        }
            
        float angleStep = segmentCount > 1 ? arcAngle / (segmentCount - 1) : 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            ShieldObject shield = shieldObjects[i];

            float relativeAngle = -arcAngle * 0.5f + angleStep * i;

            float finalAngle = shieldAngle + relativeAngle;

            float radians = finalAngle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians),0f);

            shield.transform.position = transform.position + direction * radio;

            shield.transform.rotation = Quaternion.Euler(0f,0f, finalAngle - 90f);
        }
    }

    private bool IsSegmentDestroyed(int index)
    {
        return (shieldMask & (1 << index)) != 0;
    }

    private void SetSegmentDestroyed(int index)
    {
        shieldMask |= 1 << index;
    }

    private void UpdateNetworkShield()
    {
        if (isShieldActive != netIsShieldActive)
        {
            isShieldActive = netIsShieldActive;

            if (isShieldActive)
            {
                shieldMask = netShieldMask;
                RebuildShield(netShieldSegmentCount);
            }
            else
            {
                ClearShield();
            }

            return;
        }

        if (!isShieldActive)
            return;

        if (shieldSegmentCount != netShieldSegmentCount)
        {
            shieldMask = netShieldMask;
            RebuildShield(netShieldSegmentCount);
        }

        ApplyNetworkShieldMask();
    }

    [PunRPC]
    public void ShieldTakeDamage(int segmentIndex)
    {
        if (!myView.IsMine)
            return;

        if (IsSegmentDestroyed(segmentIndex))
            return;

        DestroyShieldSegment(segmentIndex);

        myWeapon.RemoveAmmo(1);

        if (myWeapon.ammoCount <= 0)
        {
            HideShield();
        }
    }

    private void ApplyNetworkShieldMask()
    {
        for (int i = 0; i < shieldObjects.Count; i++)
        {
            bool destroyed = (netShieldMask & (1 << i)) != 0;
            shieldObjects[i].gameObject.SetActive(!destroyed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shieldAngle);
            stream.SendNext(isShieldActive);
            stream.SendNext(shieldMask);
            stream.SendNext(shieldSegmentCount);
        }
        else
        {
            netShieldAngle = (float)stream.ReceiveNext();
            netIsShieldActive = (bool)stream.ReceiveNext();
            netShieldMask = (int)stream.ReceiveNext();
            netShieldSegmentCount = (int)stream.ReceiveNext();
        }
    }
}


/*
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
        return;
    }

    ProcessShieldInput();
}

private void FixedUpdate()
{
    if (!myView.IsMine)
    {
        ShieldTransform.position = Vector3.Lerp(ShieldTransform.position, netShieldPosition, Time.deltaTime * 30);
        ShieldTransform.rotation = Quaternion.Lerp(ShieldTransform.rotation, netShieldRotation, Time.deltaTime * 30);
        return;
    }

    CalculateShieldRotation();
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

private void CalculateShieldRotation()
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
*/