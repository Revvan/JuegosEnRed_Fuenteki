using Photon.Pun;
using UnityEngine;

public class CustomTransformSync : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPos;
    private Quaternion networkRot;

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (!photonView.IsMine)

        {
            transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRot, Time.deltaTime);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}