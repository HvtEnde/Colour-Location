using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarPulseWithTags : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private Transform sonarOrigin; // Origin of the sonar pulse
    [SerializeField] private float pulseSpeed = 10f; // Speed of the pulse (units/sec)
    [SerializeField] private float pulseRepeatRate = 5f; // Pulse repeat interval (sec)
    [SerializeField] private float maxDistance = 100f; // Max pulse distance

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clipPrimair; // Clip for 'Primair' tag
    [SerializeField] private AudioClip clipSecundair; // Clip for 'Secundair' tag
    [SerializeField] private AudioClip clipTertiair; // Clip for 'Tertiair' tag

    [Header("Sequence Settings")]
    [SerializeField] private float sequenceClipDelay = 0.5f; // Delay between sequence clips
    [SerializeField] private float doubleTapTime = 0.3f; // Double-tap time window

    private AudioSource cameraAudioSource; // AudioSource for sequence playback
    private AudioClip[] sequenceClips; // Clips for the sequence
    private float lastXPressTime = -1f; // For double-tap detection

    private void Start()
    {
        if (sonarOrigin == null)
        {
            Debug.LogError("SonarOrigin not set!");
            return;
        }
        if (clipPrimair == null || clipSecundair == null || clipTertiair == null)
        {
            Debug.LogError("Please assign all three clips!");
            return;
        }

        sequenceClips = new AudioClip[] { clipPrimair, clipSecundair, clipTertiair };

        // Setup AudioSource for sequence
        cameraAudioSource = GetComponent<AudioSource>();
        if (cameraAudioSource == null)
        {
            cameraAudioSource = gameObject.AddComponent<AudioSource>();
        }
        cameraAudioSource.spatialBlend = 0f; // 2D sound for sequence

        // Initialize targets
        InitializeTargets();

        // Assign tags and clips to targets
        AssignTagsAndClipsToTargets();

        // Start sequence and pulse
        StartCoroutine(PlaySequenceAndStartPulse());
    }

    private void Update()
    {
        // Double-tap detection for X key
        if (Input.GetKeyDown(KeyCode.X))
        {
            float timeSinceLastPress = Time.time - lastXPressTime;
            if (timeSinceLastPress < doubleTapTime)
            {
                StartCoroutine(PlaySequence());
            }
            lastXPressTime = Time.time;
        }
    }

    private void InitializeTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("SonarTarget");
        foreach (GameObject target in targets)
        {
            TargetMovement movement = target.GetComponent<TargetMovement>();
            if (movement == null)
            {
                movement = target.AddComponent<TargetMovement>();
            }
            movement.sonarOrigin = sonarOrigin;
            movement.speed = 2f;
            movement.minDistance = 10f;
            movement.maxDistance = maxDistance;
            movement.Initialize();
        }
    }

    private void AssignTagsAndClipsToTargets()
    {
        string[] tags = new string[] { "Primair", "Secundair", "Tertiair" };
        GameObject[] targets = GameObject.FindGameObjectsWithTag("SonarTarget");
        foreach (GameObject target in targets)
        {
            int randomIndex = Random.Range(0, 3);
            string selectedTag = tags[randomIndex];
            target.tag = selectedTag;
            AudioSource audioSource = target.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if (selectedTag == "Primair")
                    audioSource.clip = clipPrimair;
                else if (selectedTag == "Secundair")
                    audioSource.clip = clipSecundair;
                else if (selectedTag == "Tertiair")
                    audioSource.clip = clipTertiair;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.minDistance = 1f;
                audioSource.maxDistance = maxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Changed to Logarithmic
            }
        }
    }

    private IEnumerator PlaySequenceAndStartPulse()
    {
        yield return StartCoroutine(PlaySequence());
        yield return new WaitForSeconds(3f);
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PlaySequence()
    {
        List<AudioClip> sequence = new List<AudioClip>(sequenceClips);
        Shuffle(sequence);
        foreach (AudioClip clip in sequence)
        {
            cameraAudioSource.clip = clip;
            cameraAudioSource.Play();
            yield return new WaitForSeconds(clip.length + sequenceClipDelay);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }

    private IEnumerator PulseRoutine()
    {
        string[] sonarTags = new string[] { "Primair", "Secundair", "Tertiair" };
        while (true)
        {
            List<GameObject> targets = new List<GameObject>();
            foreach (string tag in sonarTags)
            {
                targets.AddRange(GameObject.FindGameObjectsWithTag(tag));
            }
            List<AudioSource> audioSources = new List<AudioSource>();
            foreach (GameObject target in targets)
            {
                float distance = Vector3.Distance(sonarOrigin.position, target.transform.position);
                if (distance <= maxDistance)
                {
                    AudioSource audioSource = target.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSources.Add(audioSource);
                    }
                }
            }
            foreach (AudioSource audioSource in audioSources)
            {
                float distance = Vector3.Distance(sonarOrigin.position, audioSource.transform.position);
                float delay = distance / pulseSpeed;
                StartCoroutine(PlaySoundWithDelay(audioSource, delay));
            }
            yield return new WaitForSeconds(pulseRepeatRate);
        }
    }

    private IEnumerator PlaySoundWithDelay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}