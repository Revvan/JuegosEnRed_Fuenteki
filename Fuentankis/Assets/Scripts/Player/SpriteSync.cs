using UnityEngine;
using Photon.Pun;
using System.ComponentModel;

public class SpriteSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Transform Sprite;
    private SpriteRenderer spriteRender;
 
    Quaternion spriteRot = Quaternion.identity;

    private void Awake()
    {
        spriteRot = Sprite.localRotation;
        spriteRender = GetComponentInChildren<SpriteRenderer>();

        if (photonView.IsMine)
        {
            spriteRender.color = Color.deepSkyBlue;
        }
        else
        {
            spriteRender.color = Color.indianRed;
        }
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
