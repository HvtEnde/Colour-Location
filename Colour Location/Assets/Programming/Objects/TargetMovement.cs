using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public Vector3 movementAreaMin = new Vector3(-10, 0, -10);
    public Vector3 movementAreaMax = new Vector3(10, 0, 10);

    [HideInInspector] public bool isCarried = false;

    private Vector3 startPosition;
    private Vector3 currentTarget;
    private bool isMoving = true;

    void Awake()
    {
        startPosition = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        if (!isMoving || isCarried)
            return;

        // Move towards the current target
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        // Pick a new target when close
        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
            PickNewTarget();
    }

    public void PauseMovement()
    {
        isMoving = false;
    }

    public void ResumeMovement()
    {
        isMoving = true;
    }

    public void Respawn()
    {
        // Reset to a random position within the movement area
        transform.position = new Vector3(
            Random.Range(movementAreaMin.x, movementAreaMax.x),
            startPosition.y,
            Random.Range(movementAreaMin.z, movementAreaMax.z)
        );
        isCarried = false;
        ResumeMovement();
        PickNewTarget();
    }

    private void PickNewTarget()
    {
        currentTarget = new Vector3(
            Random.Range(movementAreaMin.x, movementAreaMax.x),
            startPosition.y,
            Random.Range(movementAreaMin.z, movementAreaMax.z)
        );
    }

    // Visualize movement area in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            (movementAreaMin + movementAreaMax) / 2,
            movementAreaMax - movementAreaMin
        );
    }
}
