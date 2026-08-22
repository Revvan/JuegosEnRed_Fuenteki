using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PhotonView myView;

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

        ProcessMovement();
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

    private void FixedUpdate()
    {
        // Apply velocity in FixedUpdate for physics consistency
        rb.linearVelocity = moveInput * moveSpeed; // Note: In Unity 6, 'rb.velocity' is replaced with 'rb.linearVelocity'
    }
}
