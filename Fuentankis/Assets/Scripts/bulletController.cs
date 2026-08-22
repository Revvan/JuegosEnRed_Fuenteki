using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

public class bulletController : MonoBehaviourPun
{
    private Rigidbody2D rb;
    private PhotonView myView;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!myView.IsMine)
        { return; }
        rb.AddForce(Vector2.up * 100);
    }

    // Update is called once per frame
    void Update()
    {
        if (!myView.IsMine)
        { return; }

        ProcessMovement();
    }

    void ProcessMovement()
    {

    }
}
