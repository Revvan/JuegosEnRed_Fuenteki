using UnityEngine;
using Photon.Pun;

public class BulletController : BulletBase
{
    [SerializeField] float lifetime = 5.0f;
    private float lifeStartTime = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    // Update is called once per frame
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
        //Debug.Log("trigger");

        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player");

            PhotonView otherPV = collision.gameObject.GetComponent<PhotonView>();

            if (otherPV.Owner != myView.Owner)
            {
                otherPV.RPC("RPC_DealDamage", RpcTarget.All, damage);

                //Debug.Log("not mine");
                //LifeComponent playerLife = collision.gameObject.GetComponent<LifeComponent>();
                //playerLife.DealDamage(damage);
                //playerLife.printShit("i touched you");
                PhotonNetwork.Destroy(gameObject);
            }
        }

        if (collision.gameObject.CompareTag("Shield"))
        {
            ShieldObject shieldObject = collision.gameObject.GetComponent<ShieldObject>();

            if (shieldObject.MyView.Owner != myView.Owner)
            {
                shieldObject.ShieldTakeDamage();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
