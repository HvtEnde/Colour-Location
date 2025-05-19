using UnityEngine;

public class SonarTargetInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f; // Distance within which player can interact
    private GameObject player;
    private bool isNearPlayer = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Ensure your player has "Player" tag
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        isNearPlayer = distanceToPlayer <= interactionDistance;

        if (isNearPlayer && Input.GetButtonDown("Xbox_X")) // "Xbox_X" maps to X button on Xbox controller
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // Implement pickup logic here (e.g., destroy or disable the object)
        Destroy(gameObject);
        Debug.Log("Picked up " + gameObject.name);
    }
}