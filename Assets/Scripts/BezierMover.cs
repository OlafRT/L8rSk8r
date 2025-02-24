using UnityEngine;
using System.Collections;

public class BezierMover : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target that this object will move towards. If left empty, it will search for a GameObject named 'Player' with the tag 'Player'.")]
    public Transform target;  

    [Header("Movement Settings")]
    [Tooltip("Total time (in seconds) to complete the arch movement.")]
    public float duration = 1.0f;
    [Tooltip("Additional height added to the middle of the arch.")]
    public float curveHeight = 2.0f;
    [Tooltip("Optional delay before the movement starts.")]
    public float startDelay = 0.0f;

    [Header("Activation Settings")]
    [Tooltip("Distance at which the object starts moving towards the target.")]
    public float activationDistance = 5f;

    [Header("Orbit Settings")]
    [Tooltip("Duration (in seconds) of the orbit phase (figure-8 pattern).")]
    public float orbitDuration = 3f;
    [Tooltip("Radius of the orbit (figure-8 pattern).")]
    public float orbitRadius = 2f;
    [Tooltip("Speed multiplier for the orbit phase.")]
    public float orbitSpeed = 2f;
    [Tooltip("Vertical offset for the orbit phase so it occurs in the air.")]
    public float orbitHeight = 2f;

    [Header("Orbit Randomness Settings")]
    [Tooltip("Maximum offset added randomly during orbit (applied on X, Y, and Z axes).")]
    public float orbitRandomness = 1f;

    [Header("Final Dive Settings")]
    [Tooltip("Duration (in seconds) of the final dive phase into the target.")]
    public float finalDiveDuration = 0.5f;

    private bool movementStarted = false;
    private Vector3 startPos;

    // Reference to the object's SphereCollider that will be enabled after the final dive.
    private SphereCollider sphereTrigger;

    // Unique random offsets for Perlin noise so that each instance has a unique orbit.
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    void Start()
    {
        // Assign unique noise offsets for this instance.
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);

        // If no target is assigned, try finding one with the name "Player" that also has the tag "Player".
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null && player.CompareTag("Player"))
                target = player.transform;
            else
            {
                Debug.LogError("BezierMover: No target assigned and no GameObject named 'Player' with tag 'Player' found.");
                enabled = false;
                return;
            }
        }

        // Get or add a SphereCollider and disable it initially.
        sphereTrigger = GetComponent<SphereCollider>();
        if (sphereTrigger == null)
        {
            sphereTrigger = gameObject.AddComponent<SphereCollider>();
        }
        sphereTrigger.isTrigger = true;
        sphereTrigger.enabled = false;
    }

    void Update()
    {
        if (!movementStarted)
        {
            // Check if the target is within the activation distance.
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= activationDistance)
            {
                // Record starting position and start the movement.
                startPos = transform.position;
                movementStarted = true;
                StartCoroutine(MoveAlongBezier());
            }
        }
    }

    IEnumerator MoveAlongBezier()
    {
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // Update current end and control point based on target's current position.
            Vector3 currentEnd = target.position;
            Vector3 currentControl = (startPos + currentEnd) * 0.5f + Vector3.up * curveHeight;
            transform.position = CalculateQuadraticBezierPoint(t, startPos, currentControl, currentEnd);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to the target's current position.
        transform.position = target.position;

        // Start the orbit phase.
        yield return StartCoroutine(OrbitAroundTarget());
    }

    IEnumerator OrbitAroundTarget()
    {
        float elapsed = 0f;
        Vector3 center = target.position; // Orbit center is the target's position.
        while (elapsed < orbitDuration)
        {
            float t = elapsed * orbitSpeed;
            // Base figure-8 (Lissajous) pattern.
            float baseX = orbitRadius * Mathf.Sin(t);
            float baseZ = orbitRadius * Mathf.Sin(2 * t);
            // Calculate Perlin noise offsets with unique seeds.
            float noiseX = (Mathf.PerlinNoise(elapsed * orbitSpeed + noiseOffsetX, noiseOffsetX) - 0.5f) * 2f * orbitRandomness;
            float noiseZ = (Mathf.PerlinNoise(elapsed * orbitSpeed + noiseOffsetZ, noiseOffsetZ) - 0.5f) * 2f * orbitRandomness;
            float noiseY = (Mathf.PerlinNoise(elapsed * orbitSpeed + noiseOffsetY, noiseOffsetY) - 0.5f) * 2f * orbitRandomness;
            // Set position with vertical offset plus noise on Y.
            transform.position = center + new Vector3(baseX + noiseX, orbitHeight + noiseY, baseZ + noiseZ);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // After orbiting, execute the final dive into the target.
        yield return StartCoroutine(FinalDive());
    }

    IEnumerator FinalDive()
    {
        float diveElapsed = 0f;
        Vector3 startDive = transform.position;
        Vector3 targetPos = target.position;
        while (diveElapsed < finalDiveDuration)
        {
            float t = diveElapsed / finalDiveDuration;
            transform.position = Vector3.Lerp(startDive, targetPos, t);
            diveElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // Enable the sphere trigger collider after the final dive.
        sphereTrigger.enabled = true;
    }

    /// <summary>
    /// Calculates a point on a quadratic Bézier curve.
    /// Formula: B(t) = (1-t)² * p0 + 2(1-t)t * p1 + t² * p2
    /// </summary>
    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}








