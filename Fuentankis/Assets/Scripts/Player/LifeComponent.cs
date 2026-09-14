using UnityEngine;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LifeComponent : MonoBehaviourPun, IPunObservable
{
    // Raised locally on every client by the victim's unbuffered death RPC.
    // Actor numbers are stable identifiers within the room; names are display snapshots.
    public static event System.Action<int, int, string, string, int> PlayerKilled;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents() => PlayerKilled = null;
    public int LastAttackerActorNumber { get; private set; }
    private string lastAttackerName = "";
    private int lethalCause;
    private int deathSequence;
    private int receivedDeathSequence;

    public float ActualLife=0;
    public float MaxLife=100;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image miniBar;
    [SerializeField] private float respawnTime = 3f;

    private SpriteRenderer sprite;
    private Collider2D collision;
    private PlayerMovement pMove;
    private PlayerWeaponsController pWeapCont;
    private PlayerWeapon pWep;
    private GrenadeLauncher gLaunch;
    
    private PlayerSpawner spawner;
    private GameManager match;
    private ShieldSystem shield;

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
        pWeapCont = GetComponent<PlayerWeaponsController>();
        pWep = GetComponent<PlayerWeapon>();
        gLaunch = GetComponent<GrenadeLauncher>();
        spawner = PlayerSpawner.Instance;
        match = FindFirstObjectByType<GameManager>();
        shield = GetComponent<ShieldSystem>();
        // Room members receive player replicas before choosing to enter gameplay.
        if (SceneManager.GetActiveScene().name != "MainGame") collision.enabled = false;

        if (photonView.IsMine)
        {
            ActualLife = MaxLife;
            netAlive = alive;
        }
    }

    void Update()
    {
        // Replicas retained across the RoomsScene -> MainGame transition need the new scene refs.
        if (match == null) match = FindFirstObjectByType<GameManager>();
        if (spawner == null) spawner = PlayerSpawner.Instance;
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

        text.text = string.IsNullOrWhiteSpace(myView.Owner.NickName)
            ? "Jugador" + myView.OwnerActorNr
            : myView.Owner.NickName;
        miniBar.fillAmount = ActualLife / MaxLife;
        StopActivity(alive);
    }

    private void StopActivity(bool alv)
    {
        bool canPlay = SceneManager.GetActiveScene().name == "MainGame" && (match == null || match.GameplayActive);
        if (shield != null && myView.IsMine)
        {
            if ((!canPlay || !alv) && shield.isShieldActive) shield.HideShield();
            shield.enabled = canPlay && alv;
        }
        if (!canPlay && myView.IsMine)
        {
            var body = GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
        }
        if(sprite != null) 
        {
            sprite.enabled = alv;
        }

        if (collision != null)
        {
            collision.enabled = alv && canPlay;
        }

        if (pMove != null)
        {
            pMove.enabled = alv && myView.IsMine && canPlay;
        }

        if (pWeapCont != null)
        {
            pWeapCont.enabled = alv && myView.IsMine && canPlay;
        }

        if(pWep != null)
        { 
            pWep.enabled = alv && myView.IsMine && canPlay;
        }

        if(gLaunch != null)
        {
            gLaunch.enabled = alv && myView.IsMine && canPlay;
        }
    }

    private void Die()
    {
        if(!alive) { return; }
        alive = false;
        photonView.RPC(nameof(RPC_Death), RpcTarget.All, ++deathSequence,
            LastAttackerActorNumber, lastAttackerName, PlayerName(myView.Owner), lethalCause);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        if (match != null && !match.GameplayActive) yield break;

        Transform spPoint = spawner.RandomSpawnPoint();
        ActualLife = MaxLife;
        isInvulnerable = true;

        myView.RPC("RPC_Respawn", RpcTarget.All, spPoint.position);
        myView.RPC("AddGrenade", RpcTarget.All, 1);
        myView.RPC("AddMainAmmo", RpcTarget.All, 4);

        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.5f);

        yield return new WaitForSeconds(5f);
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
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
    public void RPC_DealDamage(float damage, int cause, PhotonMessageInfo info)
    {
        if (!myView.IsMine)
        {
            return;
        }

        if((match != null && !match.GameplayActive) || isInvulnerable || !alive || ActualLife <= 0 || damage <= 0
            || float.IsNaN(damage) || float.IsInfinity(damage) || info.Sender == null)
        {
            return;
        }
        ActualLife -= damage;    
        if (ActualLife <= 0)
        {
            LastAttackerActorNumber = info.Sender.ActorNumber;
            lastAttackerName = PlayerName(info.Sender);
            lethalCause = cause == 1 ? 1 : 0;
        }

        healthBar.fillAmount = ActualLife / MaxLife;
        // Resolve accepted lethal hits now, before a frame boundary can end the round.
        if (ActualLife <= 0) Die();
    }

    [PunRPC]
    public void RPC_Death(int sequence, int attacker, string attackerName,
        string victimName, int cause, PhotonMessageInfo info)
    {
        if (info.Sender != myView.Owner || sequence <= receivedDeathSequence) return;
        receivedDeathSequence = sequence;
        alive = false;
        netAlive = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        PlayerKilled?.Invoke(attacker, myView.OwnerActorNr, attackerName, victimName, cause);
    }

    private static string PlayerName(Photon.Realtime.Player player) =>
        string.IsNullOrWhiteSpace(player.NickName) ? "Jugador" + player.ActorNumber : player.NickName;

    [PunRPC]

    public void RPC_Respawn(Vector3 pos)
    {
        transform.position = pos;
        alive = true;
        netAlive = true;
        if(myView.IsMine)
        {
            LastAttackerActorNumber = 0;
            lastAttackerName = "";
            lethalCause = 0;
            ActualLife = MaxLife;
            // UISync destroys the private HUD on remote player copies.
            healthBar.fillAmount = ActualLife / MaxLife;
        }
    }
}


