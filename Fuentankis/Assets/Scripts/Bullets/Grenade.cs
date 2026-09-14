using Photon.Pun;
using System;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class Grenade : BulletBase
{
    [SerializeField] private float granadeTimer = 2f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float bulletForce = 8f;
    private float startTime;

    [SerializeField] int bulletAmount = 8;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myView = GetComponent<PhotonView>();
        startTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime + granadeTimer < Time.realtimeSinceStartup)
        {
            myView.RPC(nameof(Explode), RpcTarget.All);
            //Debug.Log("chau grenade");
        }
    }

    [PunRPC]
    public void Explode()
    {
        if (photonView.IsMine)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerMask);

            // Apply damage to each valid enemy
            foreach (Collider2D hit in hits)
            {
                //if (hit.gameObject.CompareTag("Player"))

                PhotonView otherPV = hit.gameObject.GetComponent<PhotonView>();
                otherPV.RPC("RPC_DealDamage", RpcTarget.All, damage);
            }
            SpawnBullets();
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    public void SpawnBullets()
    {
        for (int i = 1; i <= bulletAmount; i++)
        {
            Vector3 dir = CalcBulletDirection(i, bulletAmount);
            Debug.Log(dir);
            float dirRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            BulletBase bullet = PhotonNetwork.Instantiate(bulletPrefab.name, transform.position, Quaternion.Euler(0, 0, dirRot)).GetComponent<BulletBase>();
            bullet.BulletImpulse(dir, bulletForce);
        }
        

    }

    public Vector3 CalcBulletDirection(float bulletNum, float bulletTotal)
    {
        float angle = ((bulletNum - 1f) / bulletTotal) * Mathf.PI * 2f;

        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
    }
}
