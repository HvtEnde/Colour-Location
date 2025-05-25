//using UnityEngine;

//public class ObjectInteractable : MonoBehaviour
//{
//    public float interactRadius = 2f;
//    public bool isCarried = false;

//    private TargetMovement movement;

//    void Awake()
//    {
//        movement = GetComponent<TargetMovement>();
//    }

//    public bool CanInteract(Transform player)
//    {
//        return Vector3.Distance(transform.position, player.position) <= interactRadius && !isCarried;
//    }

//    public void OnInteract()
//    {
//        isCarried = true;
//        movement.StopMovement();
//        // Optionally: highlight or change appearance
//    }

//    public void OnRelease()
//    {
//        isCarried = false;
//        movement.ResumeMovement();
//        // Optionally: remove highlight
//    }
//}
