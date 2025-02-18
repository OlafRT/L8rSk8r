using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public string mainMenuSceneName; // Set this in the Inspector

    // Field for the keybindings panel.
    public GameObject keybindingsPanel;

    private bool isPaused = false;
    private List<AudioSource> allAudioSources = new List<AudioSource>();
    private VideoPlayer[] allVideoPlayers;

    // Dictionary to keep track of video player's play state
    private Dictionary<VideoPlayer, bool> videoPlayerPlayStates = new Dictionary<VideoPlayer, bool>();

    void Start()
    {
        // Initially hide the pause menu
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Also hide the keybindings panel initially (if assigned)
        if (keybindingsPanel != null)
            keybindingsPanel.SetActive(false);

        // Find all audio sources and video players in the scene
        allAudioSources.AddRange(FindObjectsOfType<AudioSource>());
        allVideoPlayers = FindObjectsOfType<VideoPlayer>();

        // Initialize the video player play states
        foreach (var videoPlayer in allVideoPlayers)
        {
            videoPlayerPlayStates[videoPlayer] = videoPlayer.isPlaying;
        }
    }

    void Update()
    {
        // Check for input to toggle pause state
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1; // Pause/unpause time

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        // Refresh the lists if necessary
        allAudioSources = new List<AudioSource>(FindObjectsOfType<AudioSource>());
        allVideoPlayers = FindObjectsOfType<VideoPlayer>();

        // Update the video player play states for any new video players
        foreach (var vp in allVideoPlayers)
        {
            if (!videoPlayerPlayStates.ContainsKey(vp))
                videoPlayerPlayStates[vp] = vp.isPlaying;
        }

        PauseOrResumeAudio(isPaused);
        PauseOrResumeVideos(isPaused);

        // Ensure the keybindings panel is hidden when toggling pause.
        if (keybindingsPanel != null)
            keybindingsPanel.SetActive(false);

        // Show or hide pause menu canvas
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(isPaused);
    }

    void PauseOrResumeAudio(bool pause)
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null)
            {
                if (pause)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
            }
        }
    }

    void PauseOrResumeVideos(bool pause)
    {
        if (pause)
        {
            foreach (VideoPlayer videoPlayer in allVideoPlayers)
            {
                if (videoPlayer != null)
                {
                    // Store the current play state then pause
                    videoPlayerPlayStates[videoPlayer] = videoPlayer.isPlaying;
                    videoPlayer.Pause();
                }
            }
        }
        else
        {
            foreach (VideoPlayer videoPlayer in allVideoPlayers)
            {
                if (videoPlayer != null)
                {
                    // Resume only if it was playing before pause
                    if (videoPlayerPlayStates.ContainsKey(videoPlayer) && videoPlayerPlayStates[videoPlayer])
                    {
                        videoPlayer.Play();
                    }
                }
            }
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        // Ensure Time.timeScale is reset before changing scenes
        Time.timeScale = 1;

        // Hide the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load the main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Main menu scene name is not set in the Inspector!");
        }
    }

    // Function to toggle the keybindings panel.
    public void ToggleKeybindingsPanel()
    {
        if (keybindingsPanel != null)
        {
            // Toggle active state: if active, hide it; if hidden, show it.
            keybindingsPanel.SetActive(!keybindingsPanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("Keybindings panel is not assigned in the Inspector!");
        }
    }
}




