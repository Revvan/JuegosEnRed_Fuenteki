using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    private PhotonView myView;
    public Camera myCamera;

    [SerializeField] private Vector2 camOffset = Vector2.zero;
    [SerializeField] private float smoothSpeed = 1;

    private Transform cameraTransform;

    private Transform playerPos;

    bool isFollowing;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
        playerPos = GetComponent<Transform>();
        myCamera = Camera.main;
    }

    void Start()
    {
        if(myView.IsMine)
        {
            myCamera.gameObject.SetActive(true);
            StartFollowing();
        }
    }

    private void LateUpdate()
    {
        if(isFollowing)
        {
            Follow();
        }               
    }

    public void StartFollowing()
    {
        cameraTransform = Camera.main.transform;
        isFollowing = true;
        Cut();
    }

    void Follow()
    {
        Vector3 targetPos = new Vector3(playerPos.position.x + camOffset.x,
                                        playerPos.position.y + camOffset.y, 
                                        cameraTransform.position.z);

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos,
                                                smoothSpeed * Time.deltaTime);
    }

    void Cut()
    {
        cameraTransform.position = new Vector3(playerPos.position.x + camOffset.x,
                                               playerPos.position.y + camOffset.y,
                                               cameraTransform.position.z);
    }
}
