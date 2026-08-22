using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

public class bulletController : MonoBehaviourPun
{
    private Rigidbody2D rb;
    private PhotonView myView;


    [SerializeField] float damage = 100.0f;

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
        rb.AddForce(Vector2.up * 100);
    }

    // Update is called once per frame
    void Update()
    {
        KillBulletCheck();

        if (!myView.IsMine)
        { return; }

    }

    void KillBulletCheck()
    {
        if (lifeStartTime + lifetime <= Time.time)
        {
             Destroy(this.gameObject);

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            LifeComponent playerLife = collision.gameObject.GetComponent<LifeComponent>();
   
            playerLife.ActualLife -= damage;
        }
    }
}
