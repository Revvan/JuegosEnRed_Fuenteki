using Photon.Pun;
using UnityEngine;

public class ShieldObject : MonoBehaviourPun
{
    private PhotonView myView;
    private ShieldSystem shieldSystem;

    public PhotonView MyView => myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
        shieldSystem = GetComponentInParent<ShieldSystem>();
    }

    public void ShieldTakeDamage()
    {
        shieldSystem.ShieldTakeDamage();
    }
}