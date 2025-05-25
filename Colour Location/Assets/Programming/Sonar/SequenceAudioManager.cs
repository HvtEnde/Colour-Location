using UnityEngine;
using System.Collections.Generic;

public class SequenceAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class TagAudioClips
    {
        public string tag;
        public AudioClip baseClip;
        public AudioClip extraClip;
    }

    public List<TagAudioClips> tagAudioClipsList;
    public AudioClip errorClip;
    public float volume = 1f;

    public void PlayClipForTag(string tag, Vector3 position)
    {
        var entry = tagAudioClipsList.Find(t => t.tag == tag);
        if (entry != null && entry.baseClip != null)
            AudioSource.PlayClipAtPoint(entry.baseClip, position, volume);
    }

    public void PlayErrorSound(Vector3 position)
    {
        if (errorClip != null)
            AudioSource.PlayClipAtPoint(errorClip, position, volume);
    }

    public void ExpandSequence()
    {
        // Example: add extraClip to each tag, or randomize sequence, etc.
        // This is where you would expand your sequence logic.
    }
}
