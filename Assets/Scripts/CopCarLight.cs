using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopCarLight : MonoBehaviour
{
    [Header("Emission Settings")]
    [Tooltip("Delay (in seconds) before starting to toggle emission.")]
    public float emissionToggleStartDelay = 1f;
    [Tooltip("Interval (in seconds) between toggling emission.")]
    public float emissionToggleInterval = 1f;
    [Tooltip("Material on which to toggle emission.")]
    public Material targetMaterial;
    [Tooltip("Should emission be enabled at start?")]
    public bool startEmissionOn = false;
    [Tooltip("Emission color when enabled.")]
    public Color emissionOnColor = Color.white;
    [Tooltip("Multiplier for the emission intensity.")]
    public float emissionIntensity = 1f;

    // Internal state tracking
    private bool isEmissionOn;

    void Start()
    {
        // Set the initial emission state on the target material
        isEmissionOn = startEmissionOn;
        if (targetMaterial != null)
        {
            if (isEmissionOn)
                EnableEmission();
            else
                DisableEmission();
        }

        // Begin toggling the emission after the specified delay
        StartCoroutine(EmissionToggleCoroutine());
    }

    IEnumerator EmissionToggleCoroutine()
    {
        // Wait for the initial delay before starting toggling
        yield return new WaitForSeconds(emissionToggleStartDelay);
        while (true)
        {
            ToggleEmission();
            yield return new WaitForSeconds(emissionToggleInterval);
        }
    }

    void ToggleEmission()
    {
        if (targetMaterial != null)
        {
            if (isEmissionOn)
                DisableEmission();
            else
                EnableEmission();

            isEmissionOn = !isEmissionOn;
        }
    }

    void EnableEmission()
    {
        targetMaterial.EnableKeyword("_EMISSION");
        // Multiply the emission color by the intensity
        targetMaterial.SetColor("_EmissionColor", emissionOnColor * emissionIntensity);
    }

    void DisableEmission()
    {
        targetMaterial.DisableKeyword("_EMISSION");
        targetMaterial.SetColor("_EmissionColor", Color.black);
    }
}


