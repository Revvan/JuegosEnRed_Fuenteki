using Photon.Pun;
using UnityEngine;

public class ShieldObject : MonoBehaviourPun
{
    //private PhotonView myView;
    //public PhotonView MyView => myView;

    private ShieldSystem shieldSystem;
    public ShieldSystem ShieldSystem => shieldSystem;

    private int segmentIndex;
    public int SegmentIndex => segmentIndex;

    //private void Awake()
    //{
    //    myView = GetComponent<PhotonView>();
    //}

    public void Initialize(ShieldSystem system, int index)
    {
        shieldSystem = system;
        segmentIndex = index;
    }

    public void ShieldTakeDamage()
    {
        shieldSystem.ShieldTakeDamage(segmentIndex);
    }
}