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
    private float fixedY;
    private bool isStopped = false;

    public void Initialize()
    {
        if (sonarOrigin == null) return;
        dir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        fixedY = sonarOrigin.position.y;
        r_min = Random.Range(minDistance, maxDistance - 50f);
        r_max = r_min + 50f;
        r = Random.Range(r_min, r_max);
        isMovingOutward = r < (r_min + r_max) / 2f;
        transform.position = new Vector3(sonarOrigin.position.x + r * dir.x, fixedY, sonarOrigin.position.z + r * dir.z);
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (sonarOrigin == null || isStopped) return;

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

        r = Mathf.Clamp(r, minDistance, maxDistance);
        transform.position = new Vector3(sonarOrigin.position.x + r * dir.x, fixedY, sonarOrigin.position.z + r * dir.z);
    }

    public void StopMovement() => isStopped = true;
    public void ResumeMovement() => isStopped = false;

    public void Respawn()
    {
        Initialize();
        isStopped = false;
    }
}
