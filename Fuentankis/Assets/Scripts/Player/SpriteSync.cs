using UnityEngine;
using Photon.Pun;
using System.ComponentModel;

public class SpriteSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Transform Sprite;

    Quaternion spriteRot = Quaternion.identity;

    private void Awake()
    {
        spriteRot = Sprite.localRotation;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            Sprite.localRotation = Quaternion.Lerp(Sprite.localRotation, spriteRot, Time.deltaTime * 10);
        }        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Sprite.localRotation);
        }
        else
        {
            spriteRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
