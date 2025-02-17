using UnityEngine;
using System.Collections;

public class BezierMover : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target that this object will move towards. If left empty, it will search for a GameObject tagged 'Player'.")]
    public Transform target;  

    [Header("Movement Settings")]
    [Tooltip("Total time (in seconds) to complete the movement.")]
    public float duration = 1.0f;

    [Tooltip("Additional height added to the middle of the arc.")]
    public float curveHeight = 2.0f;

    [Tooltip("Optional delay before the movement starts.")]
    public float startDelay = 0.0f;

    [Header("Activation Settings")]
    [Tooltip("Distance at which the object starts moving towards the target.")]
    public float activationDistance = 5f;

    private bool movementStarted = false;
    
    // Save the starting position at activation.
    private Vector3 startPos;

    void Start()
    {
        // If no target is assigned, try finding one with the tag "Player".
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
            {
                Debug.LogError("BezierMover: No target assigned and no GameObject with tag 'Player' found.");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (!movementStarted)
        {
            // Check if the target is within the activation distance.
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= activationDistance)
            {
                // Record the position where the movement starts.
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
            
            // Update endpoint and control point each frame based on the target's current position.
            Vector3 currentEnd = target.position;
            Vector3 currentControl = (startPos + currentEnd) * 0.5f + Vector3.up * curveHeight;
            
            transform.position = CalculateQuadraticBezierPoint(t, startPos, currentControl, currentEnd);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object lands exactly at the target's current position.
        transform.position = target.position;
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



