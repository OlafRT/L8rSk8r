using System.Collections;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Tooltip("Array of audio clips to play randomly.")]
    public AudioClip[] audioClips;

    [Tooltip("Minimum time (in seconds) between sounds.")]
    public float minInterval = 10f;

    [Tooltip("Maximum time (in seconds) between sounds.")]
    public float maxInterval = 60f;

    [Tooltip("Minimum pitch (e.g., 0.95 for slight lower pitch).")]
    public float minPitch = 0.95f;

    [Tooltip("Maximum pitch (e.g., 1.05 for slight higher pitch).")]
    public float maxPitch = 1.05f;

    private AudioSource audioSource;

    void Start()
    {
        // Try to get an AudioSource on this GameObject; add one if not present.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Start the random sound coroutine.
        StartCoroutine(PlayRandomSounds());
    }

    IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            // Only play if there are audio clips assigned.
            if (audioClips != null && audioClips.Length > 0)
            {
                int index = Random.Range(0, audioClips.Length);
                AudioClip clip = audioClips[index];

                // Apply a random pitch variation.
                audioSource.pitch = Random.Range(minPitch, maxPitch);

                // Play the clip at this GameObject's position.
                audioSource.PlayOneShot(clip);
            }

            // Wait for a random interval before playing the next sound.
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}

