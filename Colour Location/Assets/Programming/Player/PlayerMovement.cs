using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float rotationX = 0f;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 lookInput = Vector2.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Called automatically by PlayerInput for "Move" action
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called automatically by PlayerInput for "Look" action
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
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        rotationX -= lookInput.y * lookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
