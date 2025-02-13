using UnityEngine;

// Attach this script to any grindable rail (with a Trigger collider).
// This rail supports grinding from either direction using two endpoints.
public class GrindRail : MonoBehaviour
{
    [Tooltip("One endpoint of the rail.")]
    public Transform railEndA;
    [Tooltip("Other endpoint of the rail.")]
    public Transform railEndB;

    private void OnTriggerEnter(Collider other)
    {
        // Look for SkateboardExtras in the parent of the colliding object.
        SkateboardExtras extras = other.GetComponentInParent<SkateboardExtras>();
        if (extras != null)
        {
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            Transform chosenEnd = railEndA;
            if (rb != null && rb.velocity != Vector3.zero)
            {
                Vector3 boardPos = other.transform.position;
                Vector3 toA = (railEndA.position - boardPos).normalized;
                Vector3 toB = (railEndB.position - boardPos).normalized;
                float dotA = Vector3.Dot(rb.velocity.normalized, toA);
                float dotB = Vector3.Dot(rb.velocity.normalized, toB);
                chosenEnd = (dotA >= dotB) ? railEndA : railEndB;
            }
            extras.StartGrind(chosenEnd);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SkateboardExtras extras = other.GetComponentInParent<SkateboardExtras>();
        if (extras != null)
        {
            extras.StopGrind();
        }
    }
}

