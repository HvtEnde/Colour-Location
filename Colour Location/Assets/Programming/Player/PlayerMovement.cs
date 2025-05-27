using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float rotationX = 0f;
    public Vector2 moveInput = Vector2.zero;
    public Vector2 lookInput = Vector2.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called by the Input System for the "Move" action
    public void OnMove(InputAction.CallbackContext context)
    {
        // Use performed and canceled to ensure proper zeroing
        if (context.performed || context.canceled)
            moveInput = context.ReadValue<Vector2>();
    }

    // Called by the Input System for the "Look" action
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }


    void Update()
    {
        // --- MOVEMENT ---
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        // --- LOOK ---
        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        // Vertical rotation (camera, clamped)
        rotationX -= lookInput.y * lookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
