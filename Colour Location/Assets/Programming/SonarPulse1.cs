using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarPulse1 : MonoBehaviour
{
    [SerializeField] private Transform sonarOrigin; // Where your "pulse" starts, you fabulous creator
    [SerializeField] private string targetTag = "SonarTarget"; // Tag for objects that should sing
    [SerializeField] private float pulseSpeed = 10f; // Speed of the pulse (units per second)
    [SerializeField] private float pulseRepeatRate = 5f; // How often the pulse fires (seconds)
    [SerializeField] private float maxDistance = 50f; // Max distance for the pulse to care about

    private void Start()
    {
        if (sonarOrigin == null)
        {
            Debug.LogError("Yo, you forgot to set the sonarOrigin, you wild rebel!");
            return;
        }
        // Kick off the pulse party
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            // Find all objects with the target tag
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
            List<AudioSource> audioSources = new List<AudioSource>();

            // Gather valid targets with AudioSource
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
                    else
                    {
                        Debug.LogWarning($"Target {target.name} has no AudioSource, you sneaky trickster!");
                    }
                }
            }

            // Trigger sounds with delay based on distance
            foreach (AudioSource audioSource in audioSources)
            {
                float distance = Vector3.Distance(sonarOrigin.position, audioSource.transform.position);
                float delay = distance / pulseSpeed; // Delay = distance / speed
                StartCoroutine(PlaySoundWithDelay(audioSource, delay));
            }

            // Wait before the next pulse
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