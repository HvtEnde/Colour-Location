using UnityEngine;
using System.Collections.Generic;

public class SequenceConnectionManager : MonoBehaviour
{
    public static SequenceConnectionManager Instance { get; private set; }

    [Header("Base Sequence Clips")]
    public List<AudioClip> basePart1Clips;
    public List<AudioClip> basePart2Clips;
    [Header("Extra Clips")]
    public AudioClip extraClip1;
    public AudioClip extraClip2;

    [Header("Audio")]
    public AudioSource audioSource; // Vul in via Inspector of laat leeg voor automatisch

    private List<string> requiredOrder = new List<string> { "Primair", "Secundair", "Tertiair" };
    private int currentStep = 0;
    private List<TargetMovement> allTargets = new List<TargetMovement>();
    private List<TargetMovement> connectedTargets = new List<TargetMovement>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        audioSource.PlayOneShot(basePart1Clips[0]);

        // Verzamel alle targets aan het begin
        foreach (string tag in requiredOrder)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objs)
            {
                TargetMovement tm = obj.GetComponent<TargetMovement>();
                if (tm != null && !allTargets.Contains(tm))
                    allTargets.Add(tm);
            }
        }
        // Voeg indien nodig automatisch een AudioSource toe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D audio
        }
    }
  

    /// <summary>
    /// Controleer of dit target nu verbonden mag worden.
    /// </summary>
    public bool CanConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        string tag = tm.gameObject.tag;
        return tag == requiredOrder[currentStep];
    }

    /// <summary>
    /// Probeer een target te verbinden. Geeft true terug als het lukt.
    /// </summary>
    public bool TryConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        if (!CanConnect(tm)) return false;

        connectedTargets.Add(tm);
        currentStep++;
        PlayTagAudio(tm);

        if (IsSequenceComplete())
        {
            OnSequenceCompleted();
        }
        return true;
    }

    /// <summary>
    /// Is de sequence compleet?
    /// </summary>
    public bool IsSequenceComplete()
    {
        return currentStep >= requiredOrder.Count;
    }

    /// <summary>
    /// Wordt aangeroepen als de sequence correct is voltooid.
    /// </summary>
    public void OnSequenceCompleted()
    {
        // Voeg extra clips toe aan de sequence
        if (extraClip1 != null && !basePart1Clips.Contains(extraClip1)) basePart1Clips.Add(extraClip1);
        if (extraClip2 != null && !basePart2Clips.Contains(extraClip2)) basePart2Clips.Add(extraClip2);

        // Reset alle objecten
        foreach (var tm in allTargets)
        {
            if (tm != null)
                tm.Respawn();
        }
        connectedTargets.Clear();
        currentStep = 0;
    }

    /// <summary>
    /// Speel audio af bij de tag van het verbonden target.
    /// </summary>
    private void PlayTagAudio(TargetMovement tm)
    {
        AudioClip clip = null;
        string tag = tm.gameObject.tag;
        if (tag == "Primair" && basePart1Clips.Count > 0)
            clip = basePart1Clips[Random.Range(0, basePart1Clips.Count)];
        else if (tag == "Secundair" && basePart2Clips.Count > 0)
            clip = basePart2Clips[Random.Range(0, basePart2Clips.Count)];
        else if (tag == "Tertiair" && extraClip1 != null)
            clip = extraClip1;

        if (clip != null)
            audioSource.PlayOneShot(clip);

        Debug.Log("Playing sound!");
    }

    /// <summary>
    /// Reset de sequence handmatig (optioneel).
    /// </summary>
    public void ResetSequence()
    {
        currentStep = 0;
        connectedTargets.Clear();
    }
}
