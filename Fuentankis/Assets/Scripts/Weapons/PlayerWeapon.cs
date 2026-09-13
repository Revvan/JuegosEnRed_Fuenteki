using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerWeapon : Weapon, IPunObservable
{
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(cannonTip.position, 0.2f);
    //}

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
    public void AddMainAmmo(int amount)
    {
        if (photonView.IsMine)
            AddAmmo(amount);
        //ammoCount += amount;
        //netAmmoCount += amount;
        //print("Here in add ammo");
    }

    [PunRPC]
    public void RemoveMainAmmo(int amount)
    {
        if (photonView.IsMine)
            RemoveAmmo(amount);
        //ammoCount -= amount;
        //netAmmoCount -= amount;
    }
}
