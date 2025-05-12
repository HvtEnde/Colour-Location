using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reeks : MonoBehaviour
{
    [SerializeField] private GameObject originPoint;
    [SerializeField] private AudioSource reeksSource;
    [SerializeField] private AudioClip[] bleeps;

    void Start()
    {
        reeksSource = GetComponent<AudioSource>();
        if (bleeps.Length == 0)
        {
            Debug.LogWarning("No bleeps assigned!");
        }
    }

    
    void Update()
    {
        
    }

    void PlayRandomBleep()
    {
        int randomIndex = Random.Range(0, bleeps.Length);
        AudioClip randomBleep = bleeps[randomIndex];
        reeksSource.PlayOneShot(randomBleep);
    }
}
