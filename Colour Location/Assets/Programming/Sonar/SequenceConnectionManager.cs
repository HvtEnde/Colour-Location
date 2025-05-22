using UnityEngine;
using System.Collections.Generic;

public class SequenceConnectionManager : MonoBehaviour
{
    public List<AudioClip> basePart1Clips;
    public List<AudioClip> basePart2Clips;
    public AudioClip extraClip1;
    public AudioClip extraClip2;

    private List<string> requiredOrder = new List<string> { "Primair", "Secundair", "Tertiair" };
    private int currentStep = 0;
    private List<TargetMovement> allTargets = new List<TargetMovement>();

    private void Start()
    {
        // Find all targets at start
        foreach (string tag in requiredOrder)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objs)
            {
                TargetMovement tm = obj.GetComponent<TargetMovement>();
                if (tm != null)
                    allTargets.Add(tm);
            }
        }
    }

    public bool CanConnect(TargetMovement tm)
    {
        if (tm == null) return false;
        string tag = tm.gameObject.tag;
        return tag == requiredOrder[currentStep];
    }

    public void Connect(TargetMovement tm)
    {
        if (tm == null) return;
        currentStep++;
    }

    public bool IsSequenceComplete()
    {
        return currentStep >= requiredOrder.Count;
    }

    public void OnSequenceCompleted()
    {
        // Add extra clips to sequence
        if (extraClip1 != null) basePart1Clips.Add(extraClip1);
        if (extraClip2 != null) basePart2Clips.Add(extraClip2);

        // Reset all objects and sequence
        foreach (var tm in allTargets)
        {
            tm.Respawn();
        }
        currentStep = 0;
    }
}
