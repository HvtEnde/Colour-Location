using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SequenceConnectionManager : MonoBehaviour
{
    public static SequenceConnectionManager Instance { get; private set; }

    [Header("Sonar Origin")]
    public Transform sonarOrigin;
    public AudioSource sonarAudioSource;

    [Header("Primair Clips")]
    public List<AudioClip> primairClips = new List<AudioClip>();
    [Header("Secundair Clips")]
    public List<AudioClip> secundairClips = new List<AudioClip>();
    [Header("Tertiair Clips")]
    public List<AudioClip> tertiairClips = new List<AudioClip>();

    [Header("Connection Sound")]
    public AudioClip connectionClip;

    [Header("Sequence Settings")]
    public float clipDelay = 2f;
    public float connectionPauseDuration = 3f;
    [Tooltip("Aantal clips waarmee de game begint. Minimaal 3.")]
    public int initialSequenceLength = 3;

    private List<string> requiredOrder = new List<string> { "Primair", "Secundair", "Tertiair" };
    private int currentStep = 0;
    private List<TargetMovement> allTargets = new List<TargetMovement>();
    private List<TargetMovement> connectedTargets = new List<TargetMovement>();
    private bool extraClipsUnlocked = false;
    private PlayerInput playerInput;
    private InputAction rumbleAction;
    private Dictionary<string, AudioClip> usedClips = new Dictionary<string, AudioClip>();
    private List<AudioClip> initialSequence = new List<AudioClip>();
    private int sequenceLength;
    public bool isPlayingSequence = false;
    private int maxAvailableClips;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        maxAvailableClips = primairClips.Count + secundairClips.Count + tertiairClips.Count;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
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
        }
        else
        {
            Debug.LogWarning("No object with tag 'Player' found.");
        }
    }

    void Start()
    {
        foreach (string tag in requiredOrder)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objs)
            {
                TargetMovement tm = obj.GetComponent<TargetMovement>();
                if (tm != null && !allTargets.Contains(tm))
                {
                    allTargets.Add(tm);
                    tm.sonarOrigin = sonarOrigin;
                    tm.InitializePosition();
                    tm.DisableFrequency();
                }
            }
        }

        if (sonarOrigin == null) Debug.LogError("SonarOrigin is not assigned!");
        if (sonarAudioSource == null) Debug.LogError("SonarAudioSource is not assigned!");
        else sonarAudioSource.spatialBlend = 0f;

        sequenceLength = Mathf.Max(3, initialSequenceLength);
        StartCoroutine(PlaySequenceAndSonar(true));
    }

    public bool CanConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        string tag = tm.gameObject.tag;
        int expectedIndex = currentStep % requiredOrder.Count;
        return tag == requiredOrder[expectedIndex];
    }

    public bool TryConnect(TargetMovement tm)
    {
        if (tm == null || !CanConnect(tm) || connectedTargets.Contains(tm)) return false;

        foreach (var target in allTargets)
        {
            target.StopTagAudio();
        }

        connectedTargets.Add(tm);
        tm.ResumeMovement();
        currentStep++;
        Debug.Log($"Connected {tm.gameObject.tag}, currentStep now: {currentStep}");

        if (connectionClip != null)
        {
            Debug.Log($"Playing connection sound: {connectionClip.name}");
            AudioSource.PlayClipAtPoint(connectionClip, tm.transform.position, 1f);
        }
        else
        {
            Debug.LogWarning("No connection clip assigned!");
        }

        PlayTagAudio(tm);

        StartCoroutine(WaitBeforePlayingSequenceAndSonar());

        if (IsSequenceComplete()) StartCoroutine(OnSequenceCompleted());
        return true;
    }

    private IEnumerator WaitBeforePlayingSequenceAndSonar()
    {
        yield return new WaitForSeconds(connectionPauseDuration);
        if (!isPlayingSequence)
        {
            yield return StartCoroutine(PlaySequenceAndSonar(false));
        }
    }

    public bool IsSequenceComplete()
    {
        return currentStep >= sequenceLength;
    }

    private IEnumerator OnSequenceCompleted()
    {
        extraClipsUnlocked = true;

        if (sequenceLength < maxAvailableClips)
        {
            sequenceLength++;
        }
        else
        {
            sequenceLength = Random.Range(3, maxAvailableClips + 1);
        }
        Debug.Log($"Sequence length updated to: {sequenceLength}");

        foreach (var tm in allTargets)
        {
            if (tm != null)
            {
                tm.TeleportToNewPosition();
                tm.isConnected = false;
            }
        }
        connectedTargets.Clear();
        currentStep = 0;
        Debug.Log("Sequence completed! Objects teleporting and sonar restarting.");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.PlaySuccessAudio();
            }
        }

        if (sonarAudioSource != null && sonarAudioSource.isPlaying)
            sonarAudioSource.Stop();
        foreach (var tm in allTargets)
        {
            if (tm != null)
            {
                tm.DisableFrequency();
                tm.StopTagAudio();
            }
        }

        yield return new WaitForSeconds(3f);

        if (!isPlayingSequence)
        {
            StartCoroutine(PlaySequenceAndSonar(true));
        }
    }

    private void PlayTagAudio(TargetMovement tm)
    {
        AudioClip clip = GetRandomClipForTag(tm.gameObject.tag);
        if (clip != null)
        {
            Debug.Log($"Playing connection sound for {tm.gameObject.tag}: {clip.name}");
            AudioSource.PlayClipAtPoint(clip, tm.transform.position, 1f);
        }
        else
        {
            Debug.LogWarning($"No clip found for connection sound of {tm.gameObject.tag}!");
        }
    }

    private IEnumerator PlaySequenceAndSonar(bool isInitial)
    {
        if (isPlayingSequence) yield break;
        isPlayingSequence = true;

        foreach (var tm in allTargets)
        {
            tm.DisableFrequency();
        }

        if (isInitial) usedClips.Clear();

        yield return StartCoroutine(PlaySonarSequence(isInitial));
        yield return StartCoroutine(ActivateSonar());

        isPlayingSequence = false;
    }

    private IEnumerator PlaySonarSequence(bool isInitial)
    {
        if (sonarAudioSource == null) yield break;

        sonarAudioSource.volume = 1f;

        if (isInitial)
        {
            initialSequence.Clear();
            for (int i = 0; i < sequenceLength; i++)
            {
                string tag = requiredOrder[i % requiredOrder.Count];
                AudioClip clip = GetRandomClipForTag(tag);
                if (clip != null)
                {
                    usedClips[tag] = clip;
                    initialSequence.Add(clip);
                }
            }
        }

        for (int i = 0; i < initialSequence.Count; i++)
        {
            AudioClip clip = initialSequence[i];
            if (clip != null)
            {
                Debug.Log($"Playing sequence clip: {clip.name}");
                sonarAudioSource.PlayOneShot(clip);
                if (Gamepad.current != null)
                {
                    Gamepad.current.SetMotorSpeeds(1.0f, 1.0f);
                    Debug.Log($"Triggering rumble for sequence clip: {clip.name} with intensity 1.0");
                    yield return new WaitForSeconds(clip.length);
                    Gamepad.current.SetMotorSpeeds(0f, 0f);
                }
                else
                {
                    Debug.LogWarning("No gamepad detected, rumble skipped.");
                }
                yield return new WaitForSeconds(clipDelay);
            }
        }
    }

    private IEnumerator ActivateSonar()
    {
        Debug.Log("Sonar fired!");
        for (int i = 0; i < sequenceLength; i++)
        {
            string tag = requiredOrder[i % requiredOrder.Count];
            Debug.Log($"Sonar fired for tag: {tag}");
            if (usedClips.ContainsKey(tag))
            {
                Debug.Log($"Sonar playing sound for {tag}");
                foreach (var tm in allTargets)
                {
                    if (tm.gameObject.tag == tag && !tm.isConnected)
                    {
                        float distance = Vector3.Distance(tm.transform.position, sonarOrigin.position);
                        float delay = distance / 10f;
                        yield return StartCoroutine(tm.PlayClipWithDistanceDelayed(usedClips[tag], delay));
                    }
                }
                if (usedClips[tag] != null)
                    yield return new WaitForSeconds(clipDelay);
            }
        }
    }

    private AudioClip GetRandomClipForTag(string tag)
    {
        List<AudioClip> clips = GetClipsForTag(tag);
        if (clips.Count > 0) return clips[Random.Range(0, clips.Count)];
        Debug.LogWarning($"No clips found for tag {tag}!");
        return null;
    }

    private List<AudioClip> GetClipsForTag(string tag)
    {
        if (tag == "Primair") return primairClips;
        if (tag == "Secundair") return secundairClips;
        if (tag == "Tertiair") return tertiairClips;
        return new List<AudioClip>();
    }
}