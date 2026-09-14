using UnityEngine;
using Photon.Pun;

public class BulletController : BulletBase
{
    [SerializeField] float lifetime = 5.0f;
    private float lifeStartTime = 0;

    private void Awake()
    {
        lifeStartTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        myView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!myView.IsMine)
        { return; }
    }

    void Update()
    {
        if (!myView.IsMine)
        { 
            return; 
        }
        KillBulletCheck();
    }

    void KillBulletCheck()
    {
        if (lifeStartTime + lifetime <= Time.time)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!myView.IsMine)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PhotonView otherPV = collision.gameObject.GetComponent<PhotonView>();

            if (otherPV.Owner != myView.Owner)
            {
                // Cluster bullets keep the grenade cause through their instantiation data.
                int cause = myView.InstantiationData is { Length: > 0 }
                    && myView.InstantiationData[0] is int value ? value : 0;
                otherPV.RPC("RPC_DealDamage", RpcTarget.All, damage, cause);

                PhotonNetwork.Destroy(gameObject);
            }
        }

        if (collision.gameObject.CompareTag("Shield"))
        {
            ShieldObject shieldObject = collision.gameObject.GetComponent<ShieldObject>();

            if (shieldObject == null)
            {
                return;
            }

            ShieldSystem shieldSystem = shieldObject.ShieldSystem;

            if (shieldSystem == null)
            {
                print("ShieldSystem is null");
                return;
            }

            if (shieldSystem.photonView.Owner != myView.Owner)
            {
                shieldSystem.photonView.RPC(nameof(ShieldSystem.ShieldTakeDamage), shieldSystem.photonView.Owner, shieldObject.SegmentIndex);

                PhotonNetwork.Destroy(gameObject);
            }
        }


    }
}


//if (collision.gameObject.CompareTag("Shield"))
//{
//    ShieldObject shieldObject = collision.gameObject.GetComponent<ShieldObject>();

//    if (shieldObject.MyView.Owner != myView.Owner)
//    {
//        shieldObject.ShieldTakeDamage();
//        PhotonNetwork.Destroy(gameObject);
//    }
//}

//if (collision.gameObject.CompareTag("Shield"))
//{
//    ShieldObject shieldObject = collision.gameObject.GetComponent<ShieldObject>();

//    if (shieldObject == null)
//    {
//        return;
//    }

//    if (shieldObject.MyView.Owner != myView.Owner)
//    {
//        shieldObject.MyView.RPC(nameof(shieldObject.ShieldSystem.ShieldTakeDamage), RpcTarget.All, shieldObject.SegmentIndex);

//        PhotonNetwork.Destroy(gameObject);
//    }
//}

//if (collision.gameObject.CompareTag("Shield"))
//{
//    if (!myView.IsMine)
//        return;

//    ShieldObject shieldObject = collision.gameObject.GetComponent<ShieldObject>();

//    if (shieldObject == null)
//        return;

//    ShieldSystem shieldSystem = shieldObject.GetComponentInParent<ShieldSystem>();

//    if (shieldSystem == null)
//        return;

//    if (shieldSystem.photonView.Owner != myView.Owner)
//    {
//        shieldSystem.photonView.RPC(nameof(ShieldSystem.ShieldTakeDamage), shieldSystem.photonView.Owner, shieldObject.SegmentIndex);

//        PhotonNetwork.Destroy(gameObject);
//    }
//}
