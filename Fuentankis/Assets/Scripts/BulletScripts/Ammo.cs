using UnityEngine;
using Photon.Pun;

public class Ammo : MonoBehaviourPunCallbacks
{
    private PhotonView myView;

    private void Start()
    {
        myView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<PlayerWeapon>(out PlayerWeapon playerWeapon))
            return;

        PhotonView playerView = playerWeapon.GetComponent<PhotonView>();

        playerView.RPC(nameof(PlayerWeapon.AddAmmo), RpcTarget.All, 1);

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