using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public Camera playerCamera;
    public LayerMask interactableMask;
    public SequenceConnectionManager sequenceManager;

    private TargetMovement selectedObject = null;
    private List<TargetMovement> connectedObjects = new List<TargetMovement>();

    public SelectionPopupUI selectionPopupUI;


    private void Update()
    {
        // Find closest interactable object
        TargetMovement nearest = FindNearestInteractable();

        // Interact (E or X)
        if (nearest != null && selectedObject == null && InteractPressed())
        {
            selectedObject = nearest;
            selectedObject.StopMovement();
            // Optionally highlight or indicate selection
        }

        // Connect (LMB or A)
        if (selectedObject != null && ConnectPressed())
        {
            TargetMovement target = FindNearestInteractable(exclude: selectedObject);
            if (target != null && !connectedObjects.Contains(target))
            {
                if (sequenceManager.CanConnect(target))
                {
                    connectedObjects.Add(target);
                    sequenceManager.Connect(target);

                    if (sequenceManager.IsSequenceComplete())
                    {
                        sequenceManager.OnSequenceCompleted();
                        connectedObjects.Clear();
                        selectedObject = null;
                    }
                }
            }
        }

        // Deselect if player walks away
        if (selectedObject != null)
        {
            float dist = Vector3.Distance(transform.position, selectedObject.transform.position);
            if (dist > interactDistance + 1f)
            {
                selectedObject.ResumeMovement();
                selectedObject = null;
                connectedObjects.Clear();
            }
        }
    }

    private TargetMovement FindNearestInteractable(TargetMovement exclude = null)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactDistance, interactableMask);
        float minDist = float.MaxValue;
        TargetMovement nearest = null;
        foreach (var hit in hits)
        {
            TargetMovement tm = hit.GetComponent<TargetMovement>();
            if (tm != null && tm != exclude)
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

    // Input system helpers
    private bool InteractPressed()
    {
        // E or X (Xbox)
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame
            || Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
    }

    private bool ConnectPressed()
    {
        // LMB or A (Xbox)
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            || Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
    }
}
