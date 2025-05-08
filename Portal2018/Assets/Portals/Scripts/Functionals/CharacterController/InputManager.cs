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
        float xRotation = Input.GetAxis("Right Stick Horizontal") * 120f * Time.deltaTime;
        float yRotation = -Input.GetAxis("Right Stick Vertical") * 120f * Time.deltaTime;
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
        moveDir += new Vector3(Input.GetAxis("Left Stick Horizontal"), 0, Input.GetAxis("Left Stick Vertical"));  //This is inverted for some weird reason

        if (moveDir.sqrMagnitude > 0f)
        {
            moved = true;
        }
#endif

        if (_autowalk) {
            moveDir += Camera.main.transform.forward;
            moved = true;
        }

        if (moved) {
            _playerController.Move(moveDir);
        }
    }

    void FixedUpdate() {
        HandleMovement();
    }
}
