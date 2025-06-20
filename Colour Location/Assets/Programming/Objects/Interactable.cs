using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float maxDistanceFromOrigin = 75f;
    public Transform moveReferencePoint;
    private Vector3 startPosition;
    private float targetDistance;
    private bool movingAway = true;
    private Vector3 movementDirection = Vector3.forward;
    private bool isMoving = true;

    [Header("Audio Settings")]
    public AudioClip frequencyClip;
    public AudioClip pickUpClip; // Nieuwe audio voor oppakken
    public AudioClip moveClip;   // Nieuwe audio voor spelerbeweging
    private AudioSource frequencySource;
    public AudioSource tagClipSource;

    [HideInInspector] public bool isCarried = false;
    [HideInInspector] public bool isConnected = false;

    private Vector3 originalScale;
    private PlayerInput playerInput;
    private InputAction rumbleAction;
    private GameObject player;
    private SequenceConnectionManager sequenceManager;
    private Vector3 lastPlayerPosition; // Voor bewegingsdetectie

    void Awake()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;

        frequencySource = gameObject.AddComponent<AudioSource>();
        if (frequencyClip == null)
        {
            Debug.LogWarning($"FrequencyClip is not assigned on {gameObject.name}, frequency audio will be disabled.");
            frequencySource.clip = null;
        }
        else
        {
            frequencySource.clip = frequencyClip;
        }
        frequencySource.loop = true;
        frequencySource.spatialBlend = 1f;
        frequencySource.rolloffMode = AudioRolloffMode.Linear;
        frequencySource.minDistance = 0f;
        frequencySource.maxDistance = 5f;
        frequencySource.volume = 0f;

        if (tagClipSource == null)
            tagClipSource = gameObject.AddComponent<AudioSource>();
        tagClipSource.spatialBlend = 1f;
        tagClipSource.playOnAwake = false;
        tagClipSource.minDistance = 1f;
        tagClipSource.maxDistance = 200f;
        tagClipSource.volume = 1f;

        player = GameObject.FindWithTag("Player");
        if (player == null)
            Debug.LogWarning("No object with tag 'Player' found. Audio and rumble may not function correctly.");
        else
            lastPlayerPosition = player.transform.position;

        sequenceManager = FindObjectOfType<SequenceConnectionManager>();
        if (sequenceManager == null)
            Debug.LogWarning("SequenceConnectionManager not found in scene, rumble during sequence may fail.");

        playerInput = player?.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            rumbleAction = playerInput.actions.FindAction("RumbleTrigger");
            if (rumbleAction == null)
                Debug.LogWarning("RumbleTrigger action not found in Input Action Asset.");
        }
        else
        {
            Debug.LogWarning("PlayerInput component not found on player object.");
        }

        targetDistance = Random.Range(-maxDistanceFromOrigin, maxDistanceFromOrigin);
        movingAway = Random.value > 0.5f;
    }

    void OnEnable()
    {
        if (rumbleAction != null) rumbleAction.performed += ctx => UpdateRumble();
    }

    void OnDisable()
    {
        if (rumbleAction != null) rumbleAction.performed -= ctx => UpdateRumble();
    }

    void Update()
    {
        if (!isMoving || isCarried || isConnected)
        {
            StopRumble();
            frequencySource.volume = 0f;
            if (frequencySource.isPlaying) frequencySource.Stop();
            return;
        }

        Vector3 targetPosition = moveReferencePoint.position + (movementDirection * targetDistance);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (Random.value < 0.05f)
            {
                movingAway = !movingAway;
                targetDistance = movingAway ? maxDistanceFromOrigin : -maxDistanceFromOrigin;
            }
        }

        if (player != null && frequencySource != null && frequencySource.clip != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= 5f)
            {
                if (!frequencySource.isPlaying) frequencySource.Play();
                frequencySource.volume = 1f - (distanceToPlayer / 5f);

                if (sequenceManager != null && !sequenceManager.isPlayingSequence)
                {
                    UpdateRumble();
                }
            }
            else
            {
                frequencySource.volume = 0f;
                if (frequencySource.isPlaying) frequencySource.Stop();
                StopRumble();
            }

            // Detecteer spelerbeweging en speel moveClip af
            if (moveClip != null && Vector3.Distance(lastPlayerPosition, player.transform.position) > 0.1f)
            {
                if (!tagClipSource.isPlaying || tagClipSource.clip != moveClip)
                {
                    tagClipSource.clip = moveClip;
                    tagClipSource.Play();
                }
                lastPlayerPosition = player.transform.position;
            }
            else if (moveClip != null && Vector3.Distance(lastPlayerPosition, player.transform.position) <= 0.1f && tagClipSource.isPlaying && tagClipSource.clip == moveClip)
            {
                tagClipSource.Stop();
            }
        }
    }

    private void UpdateRumble()
    {
        if (player != null && playerInput != null && Gamepad.current != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= 5f && sequenceManager != null && !sequenceManager.isPlayingSequence)
            {
                float intensity = 1f - (distanceToPlayer / 5f);
                Gamepad.current.SetMotorSpeeds(intensity, intensity);
            }
            else
            {
                StopRumble();
            }
        }
    }

    private void StopRumble()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }

    public void PauseMovement() => isMoving = false;
    public void ResumeMovement() => isMoving = true;

    public void Respawn()
    {
        InitializePosition();
        isCarried = false;
        isConnected = false;
        ResumeMovement();
        targetDistance = Random.Range(-maxDistanceFromOrigin, maxDistanceFromOrigin);
        movingAway = Random.value > 0.5f;
    }

    public void InitializePosition()
    {
        if (moveReferencePoint == null) return;

        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomDistance = Random.Range(-maxDistanceFromOrigin, maxDistanceFromOrigin);
            Vector3 randomPos = moveReferencePoint.position + (movementDirection * randomDistance);

            if (Vector3.Distance(randomPos, moveReferencePoint.position) <= maxDistanceFromOrigin)
            {
                transform.position = randomPos;
                targetDistance = randomDistance;
                return;
            }
        }

        transform.position = moveReferencePoint.position;
        targetDistance = 0f;
        Debug.LogWarning($"Could not find a spot within {maxDistanceFromOrigin} units for {gameObject.name}, spawning at SonarOrigin.");
    }

    public void TeleportToNewPosition()
    {
        InitializePosition();
        Debug.Log($"{gameObject.name} teleported to new position.");
    }

    public IEnumerator PlayClipWithDistanceDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tagClipSource != null && clip != null)
        {
            Debug.Log($"{gameObject.name} playing sonar sound: {clip.name}");
            float distanceToPlayer = (player != null) ? Vector3.Distance(transform.position, player.transform.position) : 10f;
            float baseVolume = 3.0f;
            float adjustedVolume = baseVolume;

            if (distanceToPlayer < 2f)
            {
                adjustedVolume = Mathf.Lerp(0.5f, baseVolume, distanceToPlayer / 2f);
            }

            tagClipSource.volume = adjustedVolume;
            tagClipSource.clip = clip;
            tagClipSource.Play();

            if (player != null && Gamepad.current != null)
            {
                float intensity = Mathf.Clamp01(1f - (distanceToPlayer / 200f));
                Debug.Log($"Rumble intensity for {gameObject.name}: {intensity}");
                Gamepad.current.SetMotorSpeeds(intensity * 0.8f, intensity * 0.8f);
                yield return new WaitForSeconds(clip.length);
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }

            tagClipSource.volume = 1f;
            Debug.Log($"Player should hear {gameObject.name} (sonar) at distance {distanceToPlayer} units.");
        }
    }

    public void EnableFrequency()
    {
        if (frequencySource != null && frequencySource.clip != null && !frequencySource.isPlaying)
            frequencySource.Play();
    }

    public void DisableFrequency()
    {
        if (frequencySource != null && frequencySource.isPlaying)
            frequencySource.Stop();
    }

    public void OnPickedUp()
    {
        transform.localScale = originalScale * 0.5f;
        if (frequencySource != null) frequencySource.Stop();
        if (tagClipSource != null) tagClipSource.Stop();
        if (pickUpClip != null) // Speel audio bij oppakken
        {
            tagClipSource.clip = pickUpClip;
            tagClipSource.Play();
        }
    }

    public void OnReleased()
    {
        transform.localScale = originalScale;
        if (frequencySource != null) EnableFrequency();
    }

    public void StopTagAudio()
    {
        if (tagClipSource != null && tagClipSource.isPlaying)
            tagClipSource.Stop();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 lineEnd = moveReferencePoint.position + (movementDirection * maxDistanceFromOrigin);
        Gizmos.DrawLine(moveReferencePoint.position, lineEnd);
    }
}