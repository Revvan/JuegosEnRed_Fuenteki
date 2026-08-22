using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public class CustomTransformSync : MonoBehaviourPun, IPunObservable
{
    Vector3 networkPos;
    Quaternion networkRot;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
