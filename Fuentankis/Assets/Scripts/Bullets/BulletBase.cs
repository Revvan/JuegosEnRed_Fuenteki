using Photon.Pun;
using UnityEngine;

public class BulletBase : MonoBehaviourPun
{
    protected Rigidbody2D rb;
    protected PhotonView myView;

    [SerializeField] protected float damage = 100.0f;

    public void BulletImpulse(Transform trns, float shootingForce)
    {
        rb.AddForce(trns.up * shootingForce, ForceMode2D.Impulse);
    }
}
