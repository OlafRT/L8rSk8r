using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource; // The AudioSource used to play music

    [Header("Music Clips")]
    public AudioClip[] thirdPersonMusic; // Array of music for third-person mode
    public AudioClip[] skateboardMusic;  // Array of music for skateboard mode
    public AudioClip combatMusic;        // Single combat music track

    [Header("Music Settings")]
    public float delayBetweenTracks = 2f; // Delay before the next track starts

    private AudioClip[] currentPlaylist;  // Current playlist of music
    private bool isCombatActive = false;  // Flag to check if combat mode is active
    private int currentTrackIndex = 0;    // Tracks which song is playing
    private bool isPlaying = false;       // Prevents multiple play calls

    void Start()
    {
        // Default to third-person music
        SetPlaylist(thirdPersonMusic);
        PlayNextTrack();
    }

    void Update()
    {
        // Check if music has finished and play the next track
        if (!musicSource.isPlaying && !isCombatActive && !isPlaying)
        {
            isPlaying = true;
            Invoke(nameof(PlayNextTrack), delayBetweenTracks);
        }
    }

    /// <summary>
    /// Sets the current music playlist.
    /// </summary>
    /// <param name="playlist">The new playlist to play.</param>
    public void SetPlaylist(AudioClip[] playlist)
    {
        currentPlaylist = playlist;
        currentTrackIndex = 0;
        PlayNextTrack();
    }

    /// <summary>
    /// Plays the next track in the current playlist.
    /// </summary>
    private void PlayNextTrack()
    {
        if (currentPlaylist.Length == 0) return;

        // Loop back to the start if at the end of the playlist
        if (currentTrackIndex >= currentPlaylist.Length) 
            currentTrackIndex = 0;

        musicSource.clip = currentPlaylist[currentTrackIndex];
        musicSource.Play();
        currentTrackIndex++;
        isPlaying = false;
    }

    /// <summary>
    /// Switches to combat music immediately.
    /// </summary>
    public void EnterCombat()
    {
        isCombatActive = true;
        musicSource.clip = combatMusic;
        musicSource.Play();
    }

    /// <summary>
    /// Exits combat and resumes the previous playlist.
    /// </summary>
    public void ExitCombat()
    {
        isCombatActive = false;
        PlayNextTrack();
    }
}

