using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pm = PlayerMovement;

public class crouching : MonoBehaviour
{

    [Header("Keybinds")]
    public static KeyCode crouchKey = KeyCode.LeftControl;
    private float verticalInput;
    private float horizontalInput;

    [Header("Movment")]
    public static float crouchSpeed = 2;
    public float crouchYScale = 0.7f;
    public float startYScale;
    public float crouchDuration = 2;
    public float crouchPercent = 0;
    public float division = 0;
    public float slideForce;
    public float slideTime;
    public float slideTimer;
    public static bool sliding;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Connections")]
    public Transform orientation;
    private PlayerMovement pm;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        startYScale = transform.localScale.y;
        sliding = false;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(crouchKey))
        {
            slideTimer = slideTime;
        }

        if (Input.GetKey(crouchKey))
        {
            float changesPerSeconds = 0.1f;
            if (crouchPercent < crouchDuration)
            {
                division = (crouchPercent / crouchDuration) / (crouchDuration * 10);
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, crouchYScale, division), transform.localScale.z);
                crouchPercent += changesPerSeconds * Time.deltaTime * 30;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                crouchPercent = crouchDuration;
            }
            
            if (Input.GetKey(pm.sprintKey) && slideTimer >= 0 && Input.GetKeyDown(crouchKey) && verticalInput > 0)
            {
                sliding = true;
            }
        }
        else if (!pm.blocked && !Input.GetKey(crouchKey))
        {
            float changesPerSeconds = 0.1f;
            if (crouchPercent > 0)
            {
                division = (crouchPercent / crouchDuration) / (crouchDuration * 10);
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, startYScale, division), transform.localScale.z);
                crouchPercent -= changesPerSeconds * Time.deltaTime * 30;
            }
            else
            {
                division = 0;
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                crouchPercent = 0;
            }
        }
    }

    void FixedUpdate()
    {
        if (sliding)
        {
            SlidingMovement();
        }
    }

    private void SlidingMovement()
    {
        moveDirection = orientation.forward * 1 + orientation.right * horizontalInput;
        if (!pm.Onslope() || rb.velocity.y > 0)
        {
            rb.AddForce(moveDirection * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
            if((slideTimer <= 0 || Input.GetKeyUp(crouchKey)) && pm.grounded)
            {
                sliding = false;
            }
        }
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(moveDirection) * slideForce, ForceMode.Force);
        }
    }
}
