using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 2f; // Pas dit aan in de Inspector
    public Transform carryPoint; // Assign in Inspector
    private TargetMovement carriedTarget = null;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction connectAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
            connectAction = playerInput.actions["Connect"];
        }
        else
        {
            Debug.LogError("PlayerInput component missing!");
        }
    }

    void OnEnable()
    {
        if (interactAction != null) interactAction.performed += OnInteract;
        if (connectAction != null) connectAction.performed += OnConnect;
    }

    void OnDisable()
    {
        if (interactAction != null) interactAction.performed -= OnInteract;
        if (connectAction != null) connectAction.performed -= OnConnect;
    }

    void Update()
    {
        if (carriedTarget != null)
        {
            carriedTarget.transform.position = carryPoint.position;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact triggered");
        if (carriedTarget != null) return;

        if (SequenceConnectionManager.Instance == null)
        {
            Debug.LogError("SequenceConnectionManager.Instance is missing!");
            return;
        }

        TargetMovement target = FindNearestTarget();
        if (target != null && !target.isCarried && Vector3.Distance(transform.position, target.transform.position) <= interactDistance)
        {
            Debug.Log($"Found target: {target.gameObject.name}");
            if (SequenceConnectionManager.Instance.CanConnect(target))
            {
                carriedTarget = target;
                carriedTarget.PauseMovement();
                carriedTarget.isCarried = true;
                carriedTarget.transform.position = carryPoint.position;
                Debug.Log("Target picked up");
            }
        }
        else
        {
            Debug.Log("No valid target found within distance");
        }
    }

    public void OnConnect(InputAction.CallbackContext ctx)
    {
        Debug.Log("Connect triggered");
        if (carriedTarget == null) return;

        if (SequenceConnectionManager.Instance == null)
        {
            Debug.LogError("SequenceConnectionManager.Instance is missing!");
            return;
        }

        if (SequenceConnectionManager.Instance.TryConnect(carriedTarget))
        {
            carriedTarget.isCarried = false;
            carriedTarget.ResumeMovement();
            carriedTarget = null;
        }
        else
        {
            // Optional: feedback for wrong order
        }
    }

    TargetMovement FindNearestTarget()
    {
        TargetMovement nearest = null;
        float minDist = Mathf.Infinity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactDistance);
        foreach (var hit in hitColliders)
        {
            TargetMovement tm = hit.GetComponent<TargetMovement>();
            if (tm != null && !tm.isCarried)
            {
                float dist = Vector3.Distance(transform.position, tm.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = tm;
                }
            }
        }

        return nearest;
    }
}