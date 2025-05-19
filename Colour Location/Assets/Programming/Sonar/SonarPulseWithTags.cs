using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarPulseWithTags : MonoBehaviour
{
    [Header("Sonar Settings")]
    [SerializeField] private Transform sonarOrigin;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseRepeatRate = 5f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Sequence Settings")]
    [SerializeField] private List<AudioClip> basePart1Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> basePart2Clips = new List<AudioClip>();
    [SerializeField] private float sequenceClipDelay = 0.5f;

    [Header("Tag Audio Clips")]
    [SerializeField] private AudioClip clipPrimair1;
    [SerializeField] private AudioClip clipPrimair2;
    [SerializeField] private AudioClip clipSecundair1;
    [SerializeField] private AudioClip clipSecundair2;
    [SerializeField] private AudioClip clipTertiair1;
    [SerializeField] private AudioClip clipTertiair2;

    private AudioSource cameraAudioSource;

    private void Start()
    {
        basePart1Clips.AddRange(Resources.LoadAll<AudioClip>("BasePart1"));
        basePart2Clips.AddRange(Resources.LoadAll<AudioClip>("BasePart2"));

        // Validate required components and assets
        if (sonarOrigin == null)
        {
            Debug.LogError("SonarOrigin not set!");
            return;
        }
        if (basePart1Clips.Count < 3 || basePart2Clips.Count < 3)
        {
            Debug.LogError("Each base part must have at least 3 audio clips!");
            return;
        }
        if (clipPrimair1 == null || clipPrimair2 == null ||
            clipSecundair1 == null || clipSecundair2 == null ||
            clipTertiair1 == null || clipTertiair2 == null)
        {
            Debug.LogError("Please assign all tag audio clips!");
            return;
        }

        // Setup cameraAudioSource for sequence
        cameraAudioSource = GetComponent<AudioSource>();
        if (cameraAudioSource == null)
        {
            cameraAudioSource = gameObject.AddComponent<AudioSource>();
        }
        cameraAudioSource.spatialBlend = 0f; // 2D sound for sequence

        // Assign clips to targets based on their tags
        AssignClipsToTargets();

        // Start sequence and sonar pulse
        StartCoroutine(PlaySequenceAndStartPulse());
    }

    private void AssignClipsToTargets()
    {
        string[] tags = new string[] { "Primair", "Secundair", "Tertiair" };
        foreach (string tag in tags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            AudioClip[] clips;
            if (tag == "Primair")
                clips = new AudioClip[] { clipPrimair1, clipPrimair2 };
            else if (tag == "Secundair")
                clips = new AudioClip[] { clipSecundair1, clipSecundair2 };
            else
                clips = new AudioClip[] { clipTertiair1, clipTertiair2 };

            foreach (GameObject target in targets)
            {
                AudioSource[] sources = target.GetComponents<AudioSource>();
                if (sources.Length < 2)
                {
                    for (int i = sources.Length; i < 2; i++)
                    {
                        target.AddComponent<AudioSource>();
                    }
                    sources = target.GetComponents<AudioSource>();
                }
                for (int i = 0; i < 2; i++)
                {
                    sources[i].clip = clips[i];
                    sources[i].spatialBlend = 1f; // 3D sound for tag clips
                    sources[i].rolloffMode = AudioRolloffMode.Logarithmic;
                }
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
        List<AudioClip> sequenceClips = new List<AudioClip>();
        sequenceClips.AddRange(basePart1Clips);
        sequenceClips.AddRange(basePart2Clips);
        Shuffle(sequenceClips);
        foreach (AudioClip clip in sequenceClips)
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
        float minInterval = 0.2f; // Adjust as needed, in seconds

        while (true)
        {
            List<GameObject> targets = new List<GameObject>();
            foreach (string tag in sonarTags)
            {
                targets.AddRange(GameObject.FindGameObjectsWithTag(tag));
            }

            // Sort targets by distance
            targets.Sort((a, b) =>
                Vector3.Distance(sonarOrigin.position, a.transform.position)
                .CompareTo(Vector3.Distance(sonarOrigin.position, b.transform.position))
            );

            float lastDelay = 0f;

            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];
                float distance = Vector3.Distance(sonarOrigin.position, target.transform.position);
                if (distance <= maxDistance)
                {
                    AudioSource[] sources = target.GetComponents<AudioSource>();
                    if (sources.Length >= 2)
                    {
                        float delay = distance / pulseSpeed;

                        // Ensure minimum interval between triggers
                        delay = Mathf.Max(delay, lastDelay + minInterval);
                        lastDelay = delay;

                        StartCoroutine(PlaySoundsSequentially(sources, delay));
                    }
                }
            }
            yield return new WaitForSeconds(pulseRepeatRate);
        }
    }


    private IEnumerator PlaySoundsSequentially(AudioSource[] sources, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (sources.Length >= 2)
        {
            // Play the first source
            if (sources[0] != null)
            {
                sources[0].Play();
                yield return new WaitForSeconds(sources[0].clip.length);
            }
            // Play the second source
            if (sources[1] != null)
            {
                sources[1].Play();
            }
        }
    }


}