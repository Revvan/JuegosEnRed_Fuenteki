using UnityEngine;
using Photon.Pun;
using TMPro;

public class LifeComponent : MonoBehaviourPun, IPunObservable
{
    public float ActualLife=0;
    public float MaxLife=100;
    [SerializeField] private TextMeshProUGUI text;
    
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
                Debug.Log("I Died " + myView.Owner);
                alive = false;
            }
        }
        text.text = "Player " + myView.Owner + ": " + ActualLife;
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

    [PunRPC]
    public void RPC_DealDamage(float damage)
    {
        ActualLife -= damage;
        NetActualLife -= damage;

    }

}


