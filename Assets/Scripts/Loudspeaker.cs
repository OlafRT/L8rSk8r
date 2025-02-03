using UnityEngine;

public class Loudspeaker : MonoBehaviour
{
    public AudioSource[] audioSources;  // Array of audio sources
    public AudioClip[] soundClips;      // Array of sound clips to play
    public float interval = 240f;       // Interval between sounds in seconds (240 seconds = 4 minutes)

    private void Start()
    {
        // Start the periodic sound playing after the interval time
        InvokeRepeating("PlayRandomSound", interval, interval);
    }

    void PlayRandomSound()
    {
        // Pick a random sound from the array
        AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];

        // Play the sound across all audio sources
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }
}


