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
    
    private Vector3 startPos;
    private Vector3 controlPoint;
    private Vector3 endPos;

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
                // Calculate movement parameters at the moment of activation.
                startPos = transform.position;
                endPos = target.position;
                controlPoint = (startPos + endPos) * 0.5f + Vector3.up * curveHeight;
                
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
            transform.position = CalculateQuadraticBezierPoint(t, startPos, controlPoint, endPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object lands exactly at the target position.
        transform.position = endPos;
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


