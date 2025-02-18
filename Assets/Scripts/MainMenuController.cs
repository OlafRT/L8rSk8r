using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string newGameSceneName; // Scene to load
    [SerializeField] private GameObject loadingScreen; // Loading screen Canvas prefab
    [SerializeField] private Slider progressBar; // Optional: Progress bar UI

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
        }

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Wait before activating the scene

        // Update loading progress (optional)
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Normalize progress
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            // Activate the scene when fully loaded
            if (asyncLoad.progress >= 0.9f)
            {
                // Optional: Wait for a short period to show the progress bar filling
                yield return new WaitForSeconds(0.5f); // Adjust this duration as needed
                asyncLoad.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }
    }

    public void OnExitGamePressed()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}

