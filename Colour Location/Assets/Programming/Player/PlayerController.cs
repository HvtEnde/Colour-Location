//using UnityEngine;

//public class PlayerController : MonoBehaviour
//{
//    public float moveSpeed = 5f;
//    public Transform carryPoint;
//    public LayerMask interactableMask;
//    public Camera mainCamera;

//    private ObjectInteractable carriedObject = null;

//    void Update()
//    {
//        HandleMovement();
//        HandleInteraction();
//        HandleConnection();
//    }

//    void HandleMovement()
//    {
//        float h = Input.GetAxis("Horizontal");
//        float v = Input.GetAxis("Vertical");
//        Vector3 move = new Vector3(h, 0, v);
//        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
//    }

//    void HandleInteraction()
//    {
//        if (carriedObject == null)
//        {
//            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton2)) // E or X
//            {
//                ObjectInteractable nearest = FindNearestInteractable();
//                if (nearest != null && nearest.CanInteract(transform))
//                {
//                    carriedObject = nearest;
//                    carriedObject.OnInteract();
//                }
//            }
//        }
//        else
//        {
//            // Carry the object
//            carriedObject.transform.position = carryPoint.position;
//        }
//    }

//    void HandleConnection()
//    {
//        if (carriedObject != null)
//        {
//            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.JoystickButton0)) // LMB or A
//            {
//                ObjectInteractable target = FindNearestInteractable();
//                if (target != null && target != carriedObject && !target.isCarried)
//                {
//                    SequenceConnectionManager.Instance.TryConnect(carriedObject, target);
//                    carriedObject.OnRelease();
//                    carriedObject = null;
//                }
//            }
//        }
//    }

//    ObjectInteractable FindNearestInteractable()
//    {
//        Collider[] hits = Physics.OverlapSphere(transform.position, 2f, interactableMask);
//        float minDist = float.MaxValue;
//        ObjectInteractable nearest = null;
//        foreach (var hit in hits)
//        {
//            ObjectInteractable obj = hit.GetComponent<ObjectInteractable>();
//            if (obj != null && !obj.isCarried)
//            {
//                float dist = Vector3.Distance(transform.position, obj.transform.position);
//                if (dist < minDist)
//                {
//                    minDist = dist;
//                    nearest = obj;
//                }
//            }
//        }
//        return nearest;
//    }
//}
