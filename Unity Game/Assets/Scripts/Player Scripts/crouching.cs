using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PM = PlayerMovement;

public class crouching : MonoBehaviour
{

    [Header("Keybinds")]
    public static KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movment")]
    public static float crouchSpeed = 2;
    public float crouchYScale = 0.7f;
    public float startYScale;
    public float crouchDuration = 2;
    public float crouchPercent = 0;
    public float division = 0;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(crouchKey) && PM.grounded)
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
        }
        else if (!PM.blocked && !Input.GetKey(crouchKey))
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
}
