using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    private float impulseDuration = 0.8f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PhotonView myView;
    public bool isImpulsing;
    private float impulseStartTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myView = GetComponent<PhotonView>();
    }


    // Update is called once per frame
    void Update()
    {
        if (!myView.IsMine)
        { return; }

        if (isImpulsing)
        {
            ProcessImpulse();
        }
        else
        {
            ProcessMovement();
        }
        
        
    }

    void ProcessMovement()
    {
        //basic player movement with WASD keys with new input system
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
        }

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void ProcessImpulse()
    {
        if (Time.time >= impulseStartTime + impulseDuration)
        {
            isImpulsing = false;
            impulseStartTime = 0;
            Debug.Log("done");
        }

    }

    //[PunRPC]
    public void StartImpulse(Vector3 force, float impulseDur)
    {
        isImpulsing = true;
        impulseStartTime = Time.time;
        rb.AddForce(force, ForceMode2D.Impulse);
        impulseDuration = impulseDur;
        Debug.Log(force);
    }

    private void FixedUpdate()
    {
        // Apply velocity in FixedUpdate for physics consistency
        if (!isImpulsing)
        {
            rb.linearVelocity = moveInput * moveSpeed; // Note: In Unity 6, 'rb.velocity' is replaced with 'rb.linearVelocity'
        }
    }
}
