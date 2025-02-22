using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireFlicker : MonoBehaviour
{
    [Header("Intensity Settings")]
    [Tooltip("The base intensity of the light.")]
    public float baseIntensity = 1.0f;
    
    [Tooltip("The maximum additional intensity variation.")]
    public float flickerRange = 0.5f;

    [Tooltip("Speed at which the light flickers. Higher values produce faster flicker.")]
    public float flickerSpeed = 1.0f;

    // Unique offset to ensure independent flickering.
    private float noiseOffset;

    // Reference to the Light component.
    private Light pointLight;

    void Start()
    {
        pointLight = GetComponent<Light>();
        if (pointLight == null)
        {
            Debug.LogError("FireFlicker: No Light component found on this GameObject.");
            enabled = false;
        }
        
        // Assign a random noise offset so that multiple lights flicker independently.
        noiseOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // Use Perlin noise with a unique offset for each instance.
        float noise = Mathf.PerlinNoise((Time.time + noiseOffset) * flickerSpeed, 0f);
        float intensityVariation = (noise - 0.5f) * 2f * flickerRange;
        pointLight.intensity = baseIntensity + intensityVariation;
    }
}

