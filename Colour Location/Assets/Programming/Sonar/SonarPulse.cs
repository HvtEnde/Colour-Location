using UnityEngine;
using System.Collections;

public class SonarPulse : MonoBehaviour
{
    public Transform centralTransform;
    public string targetTag;
    public float pulseSpeed;
    public float fireInterval;

    private void Start()
    {
        StartCoroutine(PulseFiringCoroutine());
    }

    private IEnumerator PulseFiringCoroutine()
    {
        while (true)
        {
            FirePulse();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FirePulse()
    {
        if (centralTransform == null || string.IsNullOrEmpty(targetTag))
        {
            Debug.LogWarning("Central Transform or Target Tag is not set, sweetie. Fix it!");
            return;
        }

        GameObject[] objects = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(centralTransform.position, obj.transform.position);
            float delay = distance / pulseSpeed;
            StartCoroutine(PlaySoundAfterDelay(obj, delay));
        }
    }

    private IEnumerator PlaySoundAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}