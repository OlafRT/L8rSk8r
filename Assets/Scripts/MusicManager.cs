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

    [Header("Volume Settings")]
    [Tooltip("Volume for background music.")]
    public float defaultVolume = 0.053f;
    [Tooltip("Volume for combat music.")]
    public float combatVolume = 0.1f;

    private AudioClip[] currentPlaylist;  // Current playlist of music
    private bool isCombatActive = false;  // Flag to check if combat mode is active
    private int currentTrackIndex = 0;    // Tracks which song is playing
    private bool isPlaying = false;       // Prevents multiple play calls

    // Variables to store the previous playlist state when switching to combat
    private AudioClip[] storedPlaylist;
    private int storedTrackIndex;

    void Start()
    {
        // Set the music source's volume to the default value.
        if (musicSource != null)
            musicSource.volume = defaultVolume;
        
        // Default to third-person music
        SetPlaylist(thirdPersonMusic);
        PlayNextTrack();
    }

    void Update()
    {
        // If not in combat and music has finished, schedule the next track.
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
        if (currentPlaylist == null || currentPlaylist.Length == 0)
            return;

        // Loop back to the start if at the end of the playlist.
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
        // Only store the current playlist if not already in combat.
        if (!isCombatActive)
        {
            storedPlaylist = currentPlaylist;
            storedTrackIndex = currentTrackIndex;
        }
        isCombatActive = true;
        musicSource.clip = combatMusic;
        musicSource.volume = combatVolume; // Increase volume for combat.
        musicSource.Play();
    }

    /// <summary>
    /// Exits combat and resumes the previous playlist.
    /// </summary>
    public void ExitCombat()
    {
        isCombatActive = false;
        musicSource.volume = defaultVolume; // Restore default volume.
        if (storedPlaylist != null)
        {
            currentPlaylist = storedPlaylist;
            currentTrackIndex = storedTrackIndex;
            storedPlaylist = null;
            PlayNextTrack();
        }
        else
        {
            // Fallback in case no stored playlist is available.
            PlayNextTrack();
        }
    }
}



