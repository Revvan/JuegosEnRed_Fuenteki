using UnityEngine;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class LifeComponent : MonoBehaviourPun, IPunObservable
{
    public float ActualLife=0;
    public float MaxLife=100;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float respawnTime = 3f;

    private SpriteRenderer sprite;
    private Collider2D collision;
    private PlayerMovement pMove;
    private PlayerWeapon pWeap;
    
    private PlayerSpawner spawner;

    private float NetActualLife;
    private float NetMaxLife;
    private bool netAlive = true;

    private PhotonView myView;

    private bool isInvulnerable = false;

    public bool alive=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        myView = GetComponent<PhotonView>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        collision = GetComponent<Collider2D>();
        pMove = GetComponent<PlayerMovement>();
        pWeap = GetComponent<PlayerWeapon>();
        spawner = PlayerSpawner.Instance;

        if (photonView.IsMine)
        {
            ActualLife = MaxLife;
            netAlive = alive;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (alive && ActualLife <= 0)
            {
                Debug.Log("I Died " + myView.Owner);
                Die();
            }           
        }
        else
        {
            ActualLife = NetActualLife;
            MaxLife = NetMaxLife;
            alive = netAlive;
        }

        text.text = "Player " + myView.Owner + ": " + ActualLife;
        VisualState(alive);
    }

    private void VisualState(bool alv)
    {
        if(sprite != null) 
        {
            sprite.enabled = alv;
        }

        if (collision != null)
        {
            collision.enabled = alv;
        }

        if (pMove != null)
        {
            pMove.enabled = alv && myView.IsMine;
        }

        if (pWeap != null)
        {
            pWeap.enabled = alv && myView.IsMine;
        }
    }

    private void Die()
    {
        if(!alive) { return; }
        alive = false;
        photonView.RPC("RPC_Death", RpcTarget.All);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        Transform spPoint = spawner.RandomSpawnPoint();
        ActualLife = MaxLife;
        alive = true;
        isInvulnerable = true;

        myView.RPC("RPC_Respawn", RpcTarget.All, spPoint.position);
        
        yield return new WaitForSeconds(5f);
        isInvulnerable = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ActualLife);
            stream.SendNext(MaxLife);
            stream.SendNext(alive);
        }
        else
        {
            NetActualLife = (float)stream.ReceiveNext();
            NetMaxLife = (float)stream.ReceiveNext();
            netAlive = (bool)stream.ReceiveNext();
        }
    }

    public void printShit(string shitiString) {
        Debug.Log(shitiString);
    }

    [PunRPC]
    public void RPC_DealDamage(float damage)
    {
        if (!myView.IsMine)
        {
            return;
        }

        if(isInvulnerable || !alive)
        {
            return;
        }
        ActualLife -= damage;    
    }

    [PunRPC]
    public void RPC_Death()
    {
        alive = false;
        netAlive = false;
    }

    [PunRPC]

    public void RPC_Respawn(Vector3 pos)
    {
        transform.position = pos;
        alive = true;
        netAlive = true;
        if(myView.IsMine)
        {
            ActualLife = MaxLife;
        }
    }
}


