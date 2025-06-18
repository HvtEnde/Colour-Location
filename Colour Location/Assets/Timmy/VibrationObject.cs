using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class VibrationObject : MonoBehaviour
{
    public VibrationManager vibrationManager;
    public Transform player;

    [Header("Vibration")]
    public bool sameOnBothSides = true;

    [Range(0.0f, 1.0f)]
    public float powerLeft;

    [Range(0.0f, 1.0f)]
    public float powerRight;


    [Header("Object")]
    [Range(0.0f, 10.0f)]
    public float minRadius;

    [Range(0.0f, 10.0f)]
    public float maxRadius;

    private SphereCollider collider;

    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    void Start()
    {

    }

    

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < maxRadius)
        {
            // NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
            float vibratePower = (((distance - minRadius) * (0 - 1)) / (maxRadius - minRadius)) + 1;
            vibrationManager.Vibrate(1f, vibratePower * powerLeft, vibratePower * powerRight);
        }
    }

    private void OnDisable()
    {
        vibrationManager.StopVibration();
    }

    private void OnValidate()
    {
        collider = GetComponent<SphereCollider>();
        collider.radius = maxRadius;
    }

    private void OnDrawGizmos()
    {
        // Max Radius Gizmo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minRadius);

        // Max Radius Gizmo
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
