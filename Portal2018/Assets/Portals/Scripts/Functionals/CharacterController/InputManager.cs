using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portals;

public class InputManager : MonoBehaviour {
    [SerializeField] float _mouseSensitivity = 3.0f;

    // TODO: Remove;
    [SerializeField] bool _autowalk = false;


    RigidbodyCharacterController _playerController;
    private bool _movementEnabled;

    void Awake() {
        _playerController = GetComponent<RigidbodyCharacterController>();
        _movementEnabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {

        if (!_movementEnabled) {
            return;
        }

#if UNITY_EDITOR
        float xRotation = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float yRotation = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _playerController.Rotate(xRotation, yRotation);
#else
        float xRotation = CurveInput(Input.GetAxis("Right Stick Horizontal")) * 120f * Time.deltaTime;
        float yRotation = CurveInput(-Input.GetAxis("Right Stick Vertical")) * 120f * Time.deltaTime;
        _playerController.Rotate(xRotation, yRotation);
#endif

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Cross")) {
            _playerController.Jump();
        }

        //if (Input.GetKeyDown(KeyCode.Q)) {
        //    _playerController.ToggleNoClip();
        //}
    }

    void HandleMovement() {
        Vector3 moveDir = Vector3.zero;
        bool moved = false;
        if (_movementEnabled) {
            if (Input.GetKey(KeyCode.W)) {
                moveDir += Camera.main.transform.forward;
                moved = true;
            }
            if (Input.GetKey(KeyCode.A)) {
                moveDir -= Camera.main.transform.right;
                moved = true;
            }
            if (Input.GetKey(KeyCode.S)) {
                moveDir -= Camera.main.transform.forward;
                moved = true;
            }
            if (Input.GetKey(KeyCode.D)) {
                moveDir += Camera.main.transform.right;
                moved = true;
            }
        }

#if !UNITY_EDITOR
        //Vita controls!!!
        moveDir += Camera.main.transform.forward * CurveInput(-Input.GetAxis("Left Stick Vertical"));
        moveDir += Camera.main.transform.right * CurveInput(Input.GetAxis("Left Stick Horizontal"));

        if (moveDir.sqrMagnitude > 0f)
        {
            moved = true;
        }
#endif
        /*
        if (Input.GetButtonDown("Triangle") || Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.transform.position = Vector3.up * 5f;
        }*/

        if (_autowalk) {
            moveDir += Camera.main.transform.forward;
            moved = true;
        }

        if (moved) {
            _playerController.Move(moveDir);
        }
    }

    float CurveInput(float thisInput)
    {
        return Mathf.Sign(thisInput) * (thisInput * thisInput);
    }

    void FixedUpdate() {
        HandleMovement();
    }
}
