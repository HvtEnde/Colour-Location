using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarPulseWithSequence : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private Transform sonarOrigin; // Origin of the sonar pulse
    [SerializeField] private string targetTag = "SonarTarget"; // Tag for sonar targets
    [SerializeField] private float pulseSpeed = 10f; // Speed of the pulse (units/sec)
    [SerializeField] private float pulseRepeatRate = 5f; // Pulse repeat interval (sec)
    [SerializeField] private float maxDistance = 50f; // Max pulse distance

    [Header("Sequence Settings")]
    [SerializeField] private AudioClip[] sequenceClips; // Three clips for the sequence
    [SerializeField] private float sequenceClipDelay = 0.5f; // Delay between sequence clips

    private AudioSource cameraAudioSource; // AudioSource for sequence playback

    private void Start()
    {
        if (sonarOrigin == null)
        {
            Debug.LogError("SonarOrigin not set!");
            return;
        }
        if (sequenceClips.Length != 3)
        {
            Debug.LogError("Please assign exactly 3 clips to sequenceClips!");
            return;
        }

        // Setup AudioSource for sequence
        cameraAudioSource = GetComponent<AudioSource>();
        if (cameraAudioSource == null)
        {
            cameraAudioSource = gameObject.AddComponent<AudioSource>();
        }
        cameraAudioSource.spatialBlend = 0f; // 2D sound for sequence

        // Assign clips to targets
        AssignClipsToTargets();

        // Start sequence and pulse
        StartCoroutine(PlaySequenceAndStartPulse());
    }

    private void AssignClipsToTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject target in targets)
        {
            AudioSource audioSource = target.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                int randomIndex = Random.Range(0, 3);
                audioSource.clip = sequenceClips[randomIndex];
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.minDistance = 1f;
                audioSource.maxDistance = maxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
            else
            {
                Debug.LogWarning($"Target {target.name} has no AudioSource!");
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
        while (true)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
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