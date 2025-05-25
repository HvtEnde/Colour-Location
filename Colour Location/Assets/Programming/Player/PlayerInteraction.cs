using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.E; // PC
    public KeyCode connectKey = KeyCode.Mouse0; // PC (LMB)
    public string xboxInteractButton = "joystick button 2"; // X on Xbox (adjust if needed)
    public string xboxConnectButton = "joystick button 0"; // A on Xbox (adjust if needed)

    public Transform carryPoint; // Assign in Inspector
    private TargetMovement carriedTarget = null;

    void Update()
    {
        // Make sure the SequenceConnectionManager singleton exists
        if (SequenceConnectionManager.Instance == null)
        {
            Debug.LogError("SequenceConnectionManager.Instance is missing!");
            return;
        }

        if (carriedTarget == null)
        {
            // Try to interact (pick up/select)
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(xboxInteractButton))
            {
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
        }
        else
        {
            // Carry the object
            carriedTarget.transform.position = carryPoint.position;

            // Try to connect
            if (Input.GetKeyDown(connectKey) || Input.GetKeyDown(xboxConnectButton))
            {
                if (SequenceConnectionManager.Instance.TryConnect(carriedTarget))
                {
                    carriedTarget.isCarried = false;
                    carriedTarget.ResumeMovement();
                    carriedTarget = null;
                }
                else
                {
                    // Optional: feedback for wrong order (e.g., play error sound or show UI)
                }
            }
        }
    }

    TargetMovement FindNearestTarget()
    {
        TargetMovement nearest = null;
        float minDist = Mathf.Infinity;

        // Option 1: Physics-based (requires colliders on targets)
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

        // Option 2: Fallback to FindObjectsOfType if no collider-based targets found
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
