using UnityEngine;

public class LevelLightingUpdater : MonoBehaviour
{
    [SerializeField] private Material levelSkybox;

    void Start()
    {
        // Set the level's skybox material.
        RenderSettings.skybox = levelSkybox;
        // If you're using baked GI, force an update.
        DynamicGI.UpdateEnvironment();
    }
}

