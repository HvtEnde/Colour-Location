using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 20f;
    public Transform carryPoint;
    private TargetMovement carriedTarget = null;
    public AudioClip successClip;
    private AudioSource playerAudioSource;
    private SequenceConnectionManager sequenceManager;
    
    public PlayerInput playerInput;

    void Awake()
    {
        
        if (playerInput == null)
            Debug.LogError("PlayerInput component missing!");

        playerAudioSource = gameObject.AddComponent<AudioSource>();
        if (playerAudioSource == null)
            Debug.LogError("Failed to add AudioSource component!");
        else
        {
            playerAudioSource.spatialBlend = 0f;
            playerAudioSource.playOnAwake = false;
        }

        sequenceManager = FindObjectOfType<SequenceConnectionManager>();
        if (sequenceManager == null)
        {
            Debug.LogError("SequenceConnectionManager niet gevonden in scène!");
        }

        if (carryPoint == null)
            Debug.LogWarning("CarryPoint is not assigned in Inspector!");
    }

    void Update()
    {
        if (carriedTarget != null && carryPoint != null)
        {
            carriedTarget.transform.position = carryPoint.position;
            carriedTarget.PauseMovement();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("(1) Context Performed");

            if (carriedTarget == null)
            {
                Debug.Log("(2) No Carry");

                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
                {
                    Debug.Log("(3) Raycast");
                    Debug.DrawRay(transform.position, hit.point, Color.red, 10f);

                    TargetMovement tm = hit.collider.GetComponent<TargetMovement>();
                    if (tm != null && !tm.isConnected)
                    {
                        carriedTarget = tm;
                        carriedTarget.OnPickedUp();
                        carriedTarget.PauseMovement();
                        Debug.Log($"Oppakt: {tm.gameObject.tag}");
                    }
                }
            }
        }
    }

    public void OnConnect(InputAction.CallbackContext context)
    {
        if (context.performed && carriedTarget != null && sequenceManager != null)
        {
            if (sequenceManager.TryConnect(carriedTarget))
            {
                carriedTarget.OnReleased();
                carriedTarget = null;
                PlaySuccessAudio();
                Debug.Log("Verbinding gelukt!");
            }
            else
            {
                Debug.Log("Verbinding mislukt!");
            }
        }
    }

    public void PlaySuccessAudio()
    {
        if (successClip != null && playerAudioSource != null)
        {
            playerAudioSource.clip = successClip;
            playerAudioSource.Play();
            Debug.Log("Success audio playing at player");
        }
        else
        {
            Debug.LogWarning("Success clip or player audio source not assigned!");
        }
    }
}