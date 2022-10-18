using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PM = PlayerMovement;

public class headBobController : MonoBehaviour
{

    [SerializeField] private bool _enable = true;

    [SerializeField, Range(0, 1f)] private float _amplitude = 1f;
    [SerializeField, Range(0, 30)] private float _frequency = 15.0f;

    public Transform _camera;
    public Transform _cameraHolder;
    private float _toggleSpeed = 3.0f;
    private Vector3 _startPos;
    private Rigidbody _controller;
    private int _amp = 1000;

    private void Awake()
    {
        _camera = GameObject.Find("PlayerCam").GetComponent<Transform>();
        _cameraHolder = GameObject.Find("PlayerCameraHolder").GetComponent<Transform>();
        _controller = GameObject.Find("Player").GetComponent<Rigidbody>();
        _startPos = _camera.localPosition;
    }

    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        if (speed < _toggleSpeed) return;
        if (!PM.grounded) return;
        if (!PM.sliding) 
        {
            PlayMotion(FootStepMotion());
        }
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * _frequency) * (_amplitude / _amp);
        pos.x += Mathf.Cos(Time.time * _frequency / 2) * (_amplitude / _amp) * 2;
        return pos;
    }

    private void ResetPosition()
    {
        if (_camera.localPosition == _startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1 * Time.deltaTime);
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
        pos += _cameraHolder.forward * 15.0f;
        return pos;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enable) return;

        CheckMotion();
        ResetPosition();
        //_camera.LookAt(FocusTarget());
    }
}
