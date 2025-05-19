using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    private PlayerInputActions inputActions; // The generated class instance
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0f;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable(); // Enable your action map (e.g., "Player")
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= ctx => moveInput = Vector2.zero;
        inputActions.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => lookInput = Vector2.zero;
        inputActions.Player.Disable();
    }

    private void Update()
    {
        // Movement
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Look
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
        cameraPitch -= lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }
}
