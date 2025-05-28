using UnityEngine;
using System.Collections;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public Vector3 movementAreaMin = new Vector3(-10, 0, -10);
    public Vector3 movementAreaMax = new Vector3(10, 0, 10);

    [HideInInspector] public bool isCarried = false;
    [HideInInspector] public Transform sonarOrigin;

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
    public void InitializePosition()
    {
        if (sonarOrigin == null)
        {
            Debug.LogError("SonarOrigin not set!");
            return;
        }

        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(movementAreaMin.x, movementAreaMax.x),
                startPosition.y,
                Random.Range(movementAreaMin.z, movementAreaMax.z)
            );

            if (Vector3.Distance(randomPos, sonarOrigin.position) <= 100f)
            {
                transform.position = randomPos;
                return;
            }
        }

        // Fallback als er geen geschikte positie wordt gevonden
        transform.position = sonarOrigin.position;
        Debug.LogWarning("Kon geen plek vinden binnen 100 units, spawn bij SonarOrigin.");
    }

    public void PlayClipWithDistance(AudioClip clip, float maxDistance = 20f)
    {
        if (clip == null || sonarOrigin == null) return;
        float dist = Vector3.Distance(transform.position, sonarOrigin.position);
        float volume = Mathf.Clamp01(1f - (dist / maxDistance));
        AudioSource.PlayClipAtPoint(clip, transform.position, volume);

        // Rumble effect based on distance to sonarOrigin
        if (SequenceConnectionManager.Instance != null && SequenceConnectionManager.Instance.gamepad != null)
        {
            float intensity = Mathf.Clamp01(1f - (dist / maxDistance));
            SequenceConnectionManager.Instance.Rumble(intensity, intensity, clip.length); // Rumble voor de duur van de clip
        }
    }

    public IEnumerator PlayClipWithDistanceDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayClipWithDistance(clip);
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