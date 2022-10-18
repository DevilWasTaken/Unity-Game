using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pm = PlayerMovement;
using km = KeybindManager;

public class Crouching : MonoBehaviour
{
    [Header("Movement")]
    public static float crouchSpeed = 2;
    public float crouchYScale = 0.7f;
    public float startYScale;
    public float crouchDuration = 2;
    public float crouchPercent = 0;
    public float division = 0;
    public float slideForce;
    public float slideTime;
    public float slideTimer;
    private Rigidbody rb;
    public static Vector3 setDirection;
    public static Vector3 slideDirection;
    private float verticalInput;
    private float horizontalInput;

    [Header("Connections")]
    public Transform orientation;
    private PlayerMovement pm;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(km.crouchKey))
        {
            setDirection = orientation.forward * 1;
        }

        if (Input.GetKeyDown(km.crouchKey))
        {
            slideTimer = slideTime;
            setDirection = orientation.forward * 1;
        }

        if(Input.GetKeyUp(km.crouchKey))
        {
            pm.sliding = false;
        }

        if (Input.GetKey(km.crouchKey))
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
            
            if (Input.GetKey(km.sprintKey) && slideTimer >= 0 && Input.GetKeyDown(km.crouchKey) && verticalInput > 0)
            {
                pm.sliding = true;
            }
        }
        else if (!pm.blocked && !Input.GetKey(km.crouchKey))
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
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }

    private void SlidingMovement()
    {
        if (!pm.Onslope())
        {
            rb.AddForce(setDirection * (pm.moveSpeed * 8), ForceMode.Force);
            slideTimer -= Time.deltaTime;
            if(slideTimer <= 0 && pm.grounded)
            {
                pm.sliding = false;
            }
        }
        else if (pm.Onslope() && rb.velocity.y < 0f)
        {
            rb.AddForce(pm.GetSlopeMoveDirection(setDirection) * slideForce, ForceMode.Force);
        }
        else 
        {
            pm.sliding = false;
        }
    }
}
