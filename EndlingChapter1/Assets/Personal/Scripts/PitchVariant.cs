using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchVariant : MonoBehaviour
{
    public AudioSource audioSource;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    public void PlayWithRandomPitch(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
    }
}
