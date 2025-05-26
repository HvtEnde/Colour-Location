using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 2f;
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
        // Carry the object if one is picked up
        if (carriedTarget != null)
        {
            carriedTarget.transform.position = carryPoint.position;
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (carriedTarget != null) return;

        if (SequenceConnectionManager.Instance == null)
        {
            Debug.LogError("SequenceConnectionManager.Instance is missing!");
            return;
        }

        TargetMovement target = FindNearestTarget();
        if (target != null && !target.isCarried && Vector3.Distance(transform.position, target.transform.position) <= interactDistance)
        {
            if (SequenceConnectionManager.Instance.CanConnect(target))
            {
                carriedTarget = target;
                carriedTarget.PauseMovement();
                carriedTarget.isCarried = true;
                carriedTarget.transform.position = carryPoint.position;
            }
        }
    }

    private void OnConnect(InputAction.CallbackContext ctx)
    {
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

        if (nearest == null)
        {
            foreach (var tm in FindObjectsOfType<TargetMovement>())
            {
                if (!tm.isCarried)
                {
                    float dist = Vector3.Distance(transform.position, tm.transform.position);
                    if (dist < minDist && dist <= interactDistance)
                    {
                        minDist = dist;
                        nearest = tm;
                    }
                }
            }
        }

        return nearest;
    }
}
