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

    // Reference to SonarOrigin for distance calculation
    [HideInInspector] public Transform sonarOrigin;

    void Awake()
    {
        startPosition = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        if (!isMoving || isCarried)
            return;

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
            PickNewTarget();
    }

    public void PauseMovement() => isMoving = false;
    public void ResumeMovement() => isMoving = true;

    public void Respawn()
    {
        transform.position = new Vector3(
            Random.Range(movementAreaMin.x, movementAreaMax.x),
            startPosition.y,
            Random.Range(movementAreaMin.z, movementAreaMax.z)
        );
        isCarried = false;
        ResumeMovement();
        PickNewTarget();
    }

    public void PlayClipWithDistance(AudioClip clip, float maxDistance = 20f)
    {
        if (clip == null || sonarOrigin == null) return;
        float dist = Vector3.Distance(transform.position, sonarOrigin.position);
        float volume = Mathf.Clamp01(1f - (dist / maxDistance));
        AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }

    private void PickNewTarget()
    {
        currentTarget = new Vector3(
            Random.Range(movementAreaMin.x, movementAreaMax.x),
            startPosition.y,
            Random.Range(movementAreaMin.z, movementAreaMax.z)
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            (movementAreaMin + movementAreaMax) / 2,
            movementAreaMax - movementAreaMin
        );
    }
}
