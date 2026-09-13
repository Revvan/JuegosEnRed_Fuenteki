using Photon.Pun;
using System;
using UnityEngine;

public class Grenade : BulletBase
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] private float granadeTimer = 2f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask playerMask;
    private float startTime;

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
            Debug.Log("chau granade");
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

            PhotonNetwork.Destroy(gameObject);
        }
        
    }
    

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {

    //    }
    //    else
    //    {

    //    }
    //}
}
