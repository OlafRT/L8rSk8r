using UnityEngine;
using Cinemachine;

public class SpeedTracker : MonoBehaviour
{
    [Header("References")]
    public Rigidbody skateboardRigidbody; // Assign the skateboard's Rigidbody here
    public CinemachineVirtualCamera virtualCamera; // Assign the Cinemachine camera here
    public ParticleSystem[] speedParticles; // Assign any particle systems here
    public Animator skateboardAnimator;    // Reference to the skateboard controller's Animator

    [Header("Speed Settings")]
    public float maxSpeed = 100f;     // Max speed of the board
    public float fovBase = 60f;       // Base FOV of the camera
    public float fovMax = 90f;        // Max FOV at max speed
    public float speedThreshold = 20f; // Speed at which visual effects start
    public float highSpeedAnimationThreshold = 20f; // Speed at which the fast riding animation plays

    private float currentSpeed;
    private float currentFOVVelocity; // Used with SmoothDamp

    void Update()
    {
        // Calculate the current speed of the skateboard.
        if (skateboardRigidbody != null)
        {
            currentSpeed = skateboardRigidbody.velocity.magnitude;
        }

        // Adjust the camera's field of view based on speed using Cinemachine.
        if (virtualCamera != null)
        {
            float targetFOV = Mathf.Lerp(fovBase, fovMax, currentSpeed / maxSpeed);
            virtualCamera.m_Lens.FieldOfView = Mathf.SmoothDamp(
                virtualCamera.m_Lens.FieldOfView,
                targetFOV,
                ref currentFOVVelocity,
                0.1f
            );
        }

        // Adjust particle effects based on speed.
        foreach (ParticleSystem particle in speedParticles)
        {
            if (particle != null)
            {
                var emission = particle.emission;
                bool isFast = currentSpeed > speedThreshold;

                if (isFast)
                {
                    if (!particle.isPlaying)
                    {
                        particle.Play();
                    }

                    // Increase particle emission rate based on speed.
                    emission.rateOverTime = Mathf.Lerp(0, 50, (currentSpeed - speedThreshold) / (maxSpeed - speedThreshold));
                }
                else
                {
                    if (particle.isPlaying)
                    {
                        particle.Stop();
                    }
                }
            }
        }

        // --- New Code: Update the Animator for fast riding ---
        if (skateboardAnimator != null)
        {
            // When currentSpeed exceeds the highSpeedAnimationThreshold,
            // set the "HighSpeed" bool to true so the fast riding animation is played.
            bool highSpeed = currentSpeed >= highSpeedAnimationThreshold;
            skateboardAnimator.SetBool("HighSpeed", highSpeed);
        }
    }
}



