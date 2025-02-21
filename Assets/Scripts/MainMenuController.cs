using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName; // Scene to load
    [SerializeField] private GameObject loadingScreen;  // Loading screen Canvas prefab
    [SerializeField] private Slider progressBar;          // Optional: Progress bar UI

    public void OnNewGamePressed()
    {
        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!string.IsNullOrEmpty(newGameSceneName))
        {
            StartCoroutine(LoadSceneAsync(newGameSceneName));
        }
        else
        {
            Debug.LogWarning("New Game scene name is not set in the Inspector!");
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show the loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            // Force a frame to update the UI so the loading screen appears immediately
            yield return null;
        }

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Wait before activating the scene

        // Update loading progress (optional)
        while (asyncLoad.progress < 0.9f)
        {
            if (progressBar != null)
            {
                // Normalize progress (asyncLoad.progress maxes at 0.9 before activation)
                progressBar.value = asyncLoad.progress / 0.9f;
            }
            yield return null;
        }

        // Ensure progress bar is filled
        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        // Optional: wait a bit for the player to see the progress bar filling
        yield return new WaitForSeconds(0.5f);

        // Allow scene activation
        asyncLoad.allowSceneActivation = true;

        // Wait until the scene is fully loaded and activated
        yield return new WaitUntil(() => asyncLoad.isDone);

        // Now that the scene is loaded, get it by name
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            // Set the new scene as the active scene
            SceneManager.SetActiveScene(newScene);
            // Force an update of the lighting environment (skybox, ambient light, etc.)
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' is not loaded properly.");
        }
    }

    public void OnExitGamePressed()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}



