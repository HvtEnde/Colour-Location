using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public Transform sonarOrigin;
    public float speed = 2f;
    public float minDistance = 10f;
    public float maxDistance = 100f;

    private Vector3 dir;
    private float r_min;
    private float r_max;
    private float r;
    private bool isMovingOutward;
    private float fixedY; // Fixed Y position for horizontal plane

    public void Initialize()
    {
        if (sonarOrigin == null) return;

        // Set direction on XZ plane (horizontal only)
        dir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        // Store the initial Y position of sonar origin
        fixedY = sonarOrigin.position.y;

        // Set distance bounds
        r_min = Random.Range(minDistance, maxDistance - 50f);
        r_max = r_min + 50f;

        // Initialize distance r
        r = Random.Range(r_min, r_max);
        isMovingOutward = r < (r_min + r_max) / 2f;

        // Set initial position on the horizontal plane
        transform.position = new Vector3(sonarOrigin.position.x + r * dir.x, fixedY, sonarOrigin.position.z + r * dir.z);
    }

    private void Update()
    {
        if (sonarOrigin == null) return;

        // Update distance r based on direction
        if (isMovingOutward)
        {
            r += speed * Time.deltaTime;
            if (r >= r_max)
            {
                r = r_max;
                isMovingOutward = false;
            }
        }
        else
        {
            r -= speed * Time.deltaTime;
            if (r <= r_min)
            {
                r = r_min;
                isMovingOutward = true;
            }
        }

        // Clamp distance
        r = Mathf.Clamp(r, minDistance, maxDistance);

        // Update position, keeping Y fixed
        transform.position = new Vector3(sonarOrigin.position.x + r * dir.x, fixedY, sonarOrigin.position.z + r * dir.z);
    }
}