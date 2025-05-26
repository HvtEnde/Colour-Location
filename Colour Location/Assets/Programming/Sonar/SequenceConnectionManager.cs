using UnityEngine;
using System.Collections.Generic;

public class SequenceConnectionManager : MonoBehaviour
{
    public static SequenceConnectionManager Instance { get; private set; }

    [Header("Sonar Origin")]
    public Transform sonarOrigin;

    [Header("Primair Clips (0: base1, 1: base2, 2: extra)")]
    public List<AudioClip> primairClips = new List<AudioClip>();
    [Header("Secundair Clips (0: base1, 1: base2, 2: extra)")]
    public List<AudioClip> secundairClips = new List<AudioClip>();
    [Header("Tertiair Clips (0: base1, 1: base2, 2: extra)")]
    public List<AudioClip> tertiairClips = new List<AudioClip>();

    private List<string> requiredOrder = new List<string> { "Primair", "Secundair", "Tertiair" };
    private int currentStep = 0;
    private List<TargetMovement> allTargets = new List<TargetMovement>();
    private List<TargetMovement> connectedTargets = new List<TargetMovement>();
    private bool extraClipsUnlocked = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Find all targets and assign SonarOrigin reference
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
                }
            }
        }

        // Play the sonar sequence at the start
        StartCoroutine(PlaySonarSequence());
    }

    public bool CanConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        string tag = tm.gameObject.tag;
        return tag == requiredOrder[currentStep];
    }

    public bool TryConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        if (!CanConnect(tm)) return false;

        connectedTargets.Add(tm);
        currentStep++;
        PlayTagAudio(tm);

        // Play sonar after each connection
        StartCoroutine(PlaySonarSequence());

        if (IsSequenceComplete())
        {
            OnSequenceCompleted();
        }
        return true;
    }

    public bool IsSequenceComplete()
    {
        return currentStep >= requiredOrder.Count;
    }

    public void OnSequenceCompleted()
    {
        extraClipsUnlocked = true;

        foreach (var tm in allTargets)
        {
            if (tm != null)
                tm.Respawn();
        }
        connectedTargets.Clear();
        currentStep = 0;
    }

    private void PlayTagAudio(TargetMovement tm)
    {
        AudioClip clip = GetClipForTag(tm.gameObject.tag, currentStep - 1);
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, tm.transform.position, 1f);
    }

    /// <summary>
    /// Plays the sonar sequence at the SonarOrigin and triggers all objects to play their corresponding sound.
    /// </summary>
    private System.Collections.IEnumerator PlaySonarSequence()
    {
        // Randomize order for base part 1 and base part 2
        List<string> tagsPart1 = new List<string>(requiredOrder);
        List<string> tagsPart2 = new List<string>(requiredOrder);
        Shuffle(tagsPart1);
        Shuffle(tagsPart2);

        // Play base part 1
        for (int i = 0; i < tagsPart1.Count; i++)
        {
            string tag = tagsPart1[i];
            AudioClip clip = GetClipForTag(tag, 0);
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, sonarOrigin.position, 1f);

            foreach (var tm in allTargets)
            {
                if (tm != null && tm.gameObject.tag == tag)
                    tm.PlayClipWithDistance(clip);
            }
            // Wait for the clip to finish before playing the next
            if (clip != null)
                yield return new WaitForSeconds(clip.length);
        }

        // Play base part 2
        for (int i = 0; i < tagsPart2.Count; i++)
        {
            string tag = tagsPart2[i];
            AudioClip clip = GetClipForTag(tag, 1);
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, sonarOrigin.position, 1f);

            foreach (var tm in allTargets)
            {
                if (tm != null && tm.gameObject.tag == tag)
                    tm.PlayClipWithDistance(clip);
            }
            if (clip != null)
                yield return new WaitForSeconds(clip.length);
        }
    }

    private AudioClip GetClipForTag(string tag, int partIndex)
    {
        if (tag == "Primair" && primairClips.Count > partIndex)
            return primairClips[partIndex];
        if (tag == "Secundair" && secundairClips.Count > partIndex)
            return secundairClips[partIndex];
        if (tag == "Tertiair" && tertiairClips.Count > partIndex)
            return tertiairClips[partIndex];
        return null;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
