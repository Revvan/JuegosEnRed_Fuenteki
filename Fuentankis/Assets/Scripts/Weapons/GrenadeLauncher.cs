using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeLauncher : Weapon, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ammoCount);
        }
        else
        {
            netAmmoCount = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void AddGrenade(int amount)
    {
        if (photonView.IsMine)
            AddAmmo(amount);
    }

    [PunRPC]
    public void RemoveGrenade(int amount)
    {
        if (photonView.IsMine)
            RemoveAmmo(amount);
    }
}