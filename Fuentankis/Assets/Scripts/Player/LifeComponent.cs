using UnityEngine;
using Photon.Pun;

public class LifeComponent : MonoBehaviourPun, IPunObservable
{
    public float ActualLife=0;
    public float MaxLife=100;
    
    private float NetActualLife;
    private float NetMaxLife;

    private PhotonView myView;

    public bool alive=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        myView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            ActualLife = MaxLife;
        }

    }

    void Update()
    {
        if (!alive)
        {
            return;
        }
        if (!photonView.IsMine)
        {
            ActualLife = NetActualLife;
            MaxLife = NetMaxLife;
        }
        else
        {
            if (ActualLife <= 0)
            {
                Debug.Log("I Died");
                alive = false;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ActualLife);
            stream.SendNext(MaxLife);
        }
        else
        {
            NetActualLife = (float)stream.ReceiveNext();
            NetMaxLife = (float)stream.ReceiveNext();
        }
    }

    public void printShit(string shitiString) {
        Debug.Log(shitiString);
    }


    public void DealDamage(float damage)
    {
        ActualLife -= damage;
        NetActualLife -= damage;

    }

}


