using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using crouch = Crouching;
using km = KeybindManager;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float startYScale;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float slideSpeed;
    private float horizontalInput;
    private float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    private bool readyToJump;
    public float moveSpeed;
    private float desiredSpeed;
    private float lastDesiredSpeed;
    public static bool sliding;
    public float groundDrag;
    public static bool blocked;
    private RaycastHit Hit;
    private Vector3 safeLocation;
    private float safeTimer = 1f;
    private Vector3 lastSafeLocation;
    
    [Header("Ground Checker")]
    public float playerHeight;
    public static bool grounded;

    [Header("Connections")]
    public Transform orientation;
    public TextMeshProUGUI textElement;
    public Transform player;
    public Transform DebugSphere;

    [Header("Slope Handling")]
    public float maxSlope;
    private RaycastHit slopeHit;
    private bool exitingSlope;
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
        sliding = false;
    }

    
    void Update()
    {
        input();
        SpeedControl();
        StateHandler();
        blocked = Physics.SphereCast(transform.position, 0.5f, Vector3.up, out Hit, playerHeight);
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
        
        safeTimer -= Time.deltaTime;
        if (safeTimer <= 0f && grounded)
        {
            lastSafeLocation = rb.position;
            safeTimer = 1f;
        }

        if (rb.position.y < -10f)
        {
            rb.velocity = new Vector3(0, 0, 0);
            rb.position = lastSafeLocation;
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
        if (Input.GetKey(km.jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (sliding)
        {
            state = MovementState.sliding;
            if (Onslope() && rb.velocity.y < 0.1f)
            {
                desiredSpeed = slideSpeed;
            }
            else
            {
                desiredSpeed = sprintSpeed;
            }
        }
        else if (Input.GetKey(km.crouchKey) || ((state == MovementState.crouching || state == MovementState.sliding) && blocked))
        {
            state = MovementState.crouching;
            desiredSpeed = crouch.crouchSpeed;
        }
        else if(grounded && Input.GetKey(km.sprintKey))
        {
            state = MovementState.sprinting;
            desiredSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        if (Mathf.Abs(desiredSpeed - lastDesiredSpeed) > 4 && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothSpeed());
        }
        else
        {
            moveSpeed = desiredSpeed;
        }

        lastDesiredSpeed = desiredSpeed;
    }

    private IEnumerator SmoothSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredSpeed - moveSpeed);
        float startValue = moveSpeed;
        
        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredSpeed, time/difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredSpeed;
    }

    private void MovePlayer()
    {
        if (!sliding)
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        }
        else
        {
            moveDirection = crouch.setDirection;
        }

        if (Walkable() && Onslope() && !exitingSlope)
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

        if (Onslope() && !Walkable()) 
        {
            rb.AddForce(Vector3.down * (moveSpeed * 9.5f), ForceMode.Force);
        }
        rb.useGravity = !Onslope() || !Walkable();
    }

    private void SpeedControl()
    {
        if (Walkable() && Onslope() && !exitingSlope)
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
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            return angle != 0;
        }
        return false;
    }

    public bool Walkable()
    {
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        return angle <= maxSlope;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
