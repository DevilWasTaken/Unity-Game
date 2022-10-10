using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using crouch = crouching;

public class PlayerMovement : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public static KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float startYScale;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private float horizontalInput;
    private float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    private bool readyToJump;
    public float groundDrag;
    public static bool blocked;
    private RaycastHit Hit;
    
    [Header("Ground Checker")]
    public float playerHeight;
    public static bool grounded;

    [Header("Connections")]
    public Transform orientation;
    public Text textElement;
    public Transform player;
    public Transform Debug;

    [Header("Slope Handling")]
    public float maxSlope;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    private float moveSpeed;
    public static MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    
    void Update()
    {
        input();
        SpeedControl();
        StateHandler();
        blocked = Physics.SphereCast(transform.position, 0.5f, Vector3.up, out Hit, playerHeight);
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f);
        //Debug.position = (Raycast here).point; this is for debuging raycasts
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void input()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouch.crouchKey) || (state == MovementState.crouching && blocked))
        {
            state = MovementState.crouching;
            moveSpeed = crouch.crouchSpeed;
        }
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (Onslope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 10f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10f, ForceMode.Force);
        }
        rb.useGravity = !Onslope();
    }

    private void SpeedControl()
    {
        if (Onslope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        Vector3 Speed = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        textElement.text = state + " | Speed: " + Speed.magnitude;
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool Onslope()
    {
        if(Physics.SphereCast(transform.position, 0.5f, Vector3.down, out slopeHit, playerHeight * 0.1f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlope && angle != 0;
        }

        return false;
    }
       
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
