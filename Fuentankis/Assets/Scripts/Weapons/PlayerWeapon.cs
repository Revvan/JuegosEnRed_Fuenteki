using Photon.Pun;

public class PlayerWeapon : Weapon, IPunObservable
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
    public void AddMainAmmo(int amount)
    {
        if (photonView.IsMine)
            AddAmmo(amount);
    }

    [PunRPC]
    public void RemoveMainAmmo(int amount)
    {
        if (photonView.IsMine)
            RemoveAmmo(amount);
    }
}
