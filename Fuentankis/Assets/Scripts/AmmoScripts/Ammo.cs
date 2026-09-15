using UnityEngine;
using Photon.Pun;

public class Ammo : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool isGrenade = false;
    private PhotonView myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<PlayerWeapon>(out PlayerWeapon playerWeapon) || !collision.gameObject.TryGetComponent<GrenadeLauncher>(out GrenadeLauncher grenadeLauncher))
            return;

        PhotonView playerView = playerWeapon.GetComponent<PhotonView>();
        if (isGrenade)
        {
            playerView.RPC(nameof(GrenadeLauncher.AddGrenade), RpcTarget.All, 1);
        }
        else
        {
            playerView.RPC(nameof(PlayerWeapon.AddMainAmmo), RpcTarget.All, 1);
        }

        myView.RPC(nameof(DestroyAmmo), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void DestroyAmmo()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
