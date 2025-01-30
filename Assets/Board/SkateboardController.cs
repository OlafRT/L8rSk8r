using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
    public class SkateboardController : MonoBehaviour
    {
        [Header("Push/Brake Settings")]
        public float pushForce = 5f;          // Impulse force when pressing push
        public float maxSpeed = 15f;          // Maximum speed
        public float brakeForce = 50f;        // Force applied when braking

        [Header("Ground Stick / Suspension")]
        [Tooltip("How far to raycast below the board.")]
        public float suspensionRayLength = 0.5f;
        [Tooltip("How strong the spring push is when near the ground.")]
        public float suspensionStrength = 200f;
        [Tooltip("Damping to prevent bouncing.")]
        public float suspensionDamp = 5f;
        [Tooltip("Optional offset so the board doesn't float (e.g. 0.05 => sits 5cm lower).")]
        public float rideHeightAdjustment = 0.0f;
        public LayerMask groundLayer;

        [Header("Turning")]
        public float turnSpeed = 100f; // Angular speed for turning left/right

        [Header("Fancy Visuals")]
        public Transform frontWheel;
        public Transform backHinge;
        public Transform plank;
        public float plankTiltAngle = 10f;
        public float hingeRotationSpeed = 10f;
        public float maxHingeAngle = 30f;
        public float wheelRotationSpeed = 5f;

        [Header("Physics")]
        [Tooltip("Drag applied when on the ground.")]
        public float dragOnGround = 0.2f;
        [Tooltip("Drag applied in mid-air.")]
        public float dragInAir = 0.0f;
        [Tooltip("Extra gravity multiplier for a heavier feel.")]
        public float gravityMultiplier = 1.0f;

        [Header("Animation")]
        public Animator animator;
        public string pushAnimationTrigger = "Push";

        [Header("Ground Alignment")]
        [Tooltip("Speed for slerp alignment to ground when grounded.")]
        public float groundAlignSpeed = 10f; 

        [Header("Lateral Friction (No-Drift)")]
        [Tooltip("0 = remove all sideways velocity each frame; 1 = remove none.")]
        public float lateralVelocityRetention = 0.0f; 

        [Header("In-Air Alignment")]
        [Tooltip("If true, the board will slowly rotate to face velocity in air.")]
        public bool alignInAir = true;
        [Tooltip("Slerp speed for air alignment.")]
        public float airAlignSpeed = 1f;      

        // Private references
        private Rigidbody rb;
        private PlayerInput playerInput;
        private StarterAssetsInputs _input;

        // Ground check
        private bool isGrounded;
        private RaycastHit groundHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            // Disable built-in gravity; we apply custom gravity in FixedUpdate.
            rb.useGravity = false;
            rb.drag = dragOnGround;
            rb.angularDrag = 0.05f;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother motion

            playerInput = GetComponent<PlayerInput>();
            _input = GetComponent<StarterAssetsInputs>();
        }

        private void Update()
        {
            // Handle purely visual or input-based checks in Update.
            AnimateBackHinge();
            TiltPlank();
            RotateWheels();
        }

        private void FixedUpdate()
        {
            // 1) Ground check (raycast)
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                out groundHit,
                suspensionRayLength,
                groundLayer
            );

            // 2) Custom gravity
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

            // 3) Stick to ground via spring force if grounded
            if (isGrounded)
            {
                ApplySuspension();
                rb.drag = dragOnGround;

                // Align to ground normal
                AlignToGroundNormal(groundHit.normal);

                // Remove drifting if desired
                ApplyNoDrift();
            }
            else
            {
                // In mid-air, reduce drag for more natural flight
                rb.drag = dragInAir;

                // Optionally align to velocity in mid-air
                if (alignInAir)
                {
                    AlignBoardInAir();
                }
            }

            // 4) Handle pushing
            HandlePush();

            // 5) Handle braking
            HandleBraking();

            // 6) Handle turning
            HandleTurning();

            // 7) Enforce max speed
            LimitSpeed();
        }

        /// <summary>
        /// Applies a spring force to keep the board near the ground, 
        /// accounting for rideHeightAdjustment so it doesn't hover too high.
        /// </summary>
        private void ApplySuspension()
        {
            float distance = groundHit.distance - rideHeightAdjustment;
            // Don’t let distance go below zero (meaning we are “through” the ground).
            if (distance < 0f) distance = 0f;

            // If distance < suspensionRayLength, we push up.
            float springOffset = suspensionRayLength - distance;
            if (springOffset < 0f) return; // Already above suspensionRayLength => no spring needed.

            // Hooke’s law spring: F = k * x
            float springForce = springOffset * suspensionStrength;

            // Damping based on velocity along board’s up
            float verticalVelocity = Vector3.Dot(rb.velocity, transform.up);
            float damping = suspensionDamp * verticalVelocity;

            Vector3 force = transform.up * (springForce - damping);
            rb.AddForce(force, ForceMode.Force);
        }

        /// <summary>
        /// Slowly aligns the board’s “up” to the ground normal while grounded.
        /// </summary>
        private void AlignToGroundNormal(Vector3 groundNormal)
        {
            // The target rotation realigning up-axis to ground
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;

            // Slerp at groundAlignSpeed
            Quaternion finalRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                groundAlignSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(finalRotation);
        }

        /// <summary>
        /// If in the air, optionally make the board slowly face its velocity vector (pitching up/down).
        /// </summary>
        private void AlignBoardInAir()
        {
            Vector3 vel = rb.velocity;
            // If too small velocity, do nothing
            if (vel.sqrMagnitude < 0.1f) return;

            // The board's forward should align with actual velocity (full 3D).
            // If you want to keep the board from flipping upside down, 
            // you can use Vector3.up as the second parameter:
            Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);

            Quaternion finalRot = Quaternion.Slerp(
                rb.rotation,
                targetRot,
                airAlignSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(finalRot);
        }

        /// <summary>
        /// Removes or reduces velocity perpendicular to the board's forward axis (no drift).
        /// </summary>
        private void ApplyNoDrift()
        {
            Vector3 velocity = rb.velocity;
            if (velocity.sqrMagnitude < 0.0001f) return;

            // Separate velocity into forward vs sideways
            Vector3 forwardDir = transform.forward;
            Vector3 forwardComponent = Vector3.Project(velocity, forwardDir);
            Vector3 sidewaysComponent = velocity - forwardComponent;

            // Keep some fraction of sideways (controlled by lateralVelocityRetention)
            Vector3 newVelocity = forwardComponent + sidewaysComponent * lateralVelocityRetention;
            rb.velocity = newVelocity;
        }

        /// <summary>
        /// Impulse forward when space is pressed and grounded.
        /// </summary>
        private void HandlePush()
        {
            if (_input.jump && isGrounded)
            {
                // Instant impulse in forward direction
                rb.AddForce(transform.forward * pushForce, ForceMode.VelocityChange);

                // Trigger push animation
                animator?.SetTrigger(pushAnimationTrigger);

                // Prevent repeated triggers in the same frame
                _input.jump = false;
            }
        }

        /// <summary>
        /// Braking: applying force opposite to velocity on "S" or down input.
        /// </summary>
        private void HandleBraking()
        {
            float moveInput = playerInput.actions["Move"].ReadValue<Vector2>().y;

            // If pressing down while moving forward
            if (isGrounded && moveInput < 0f && rb.velocity.magnitude > 0.1f)
            {
                Vector3 brakeDirection = -rb.velocity.normalized;
                rb.AddForce(brakeDirection * brakeForce, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Steering left/right (Y-axis rotation) when grounded.
        /// </summary>
        private void HandleTurning()
        {
            float turnInput = playerInput.actions["Move"].ReadValue<Vector2>().x;

            if (isGrounded && Mathf.Abs(turnInput) > 0.1f)
            {
                float angleThisFrame = turnInput * turnSpeed * Time.fixedDeltaTime;
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, angleThisFrame, 0f));
            }
        }

        /// <summary>
        /// Clamps total velocity to maxSpeed to prevent infinite acceleration.
        /// </summary>
        private void LimitSpeed()
        {
            Vector3 velocity = rb.velocity;
            float speed = velocity.magnitude;
            if (speed > maxSpeed)
            {
                rb.velocity = velocity.normalized * maxSpeed;
            }
        }

        // ----------------------------- VISUAL EXTRAS ----------------------------- //

        /// <summary>
        /// Rotates the back hinge visually when turning.
        /// </summary>
        private void AnimateBackHinge()
        {
            float turnInput = playerInput.actions["Move"].ReadValue<Vector2>().x;

            float targetHingeAngle = (isGrounded && Mathf.Abs(turnInput) > 0.1f)
                ? turnInput * maxHingeAngle
                : 0f;

            if (backHinge)
            {
                backHinge.localRotation = Quaternion.Lerp(
                    backHinge.localRotation,
                    Quaternion.Euler(0f, targetHingeAngle, 0f),
                    Time.deltaTime * hingeRotationSpeed
                );
            }
        }

        /// <summary>
        /// Tilts the plank side-to-side when turning, for a lean effect.
        /// </summary>
        private void TiltPlank()
        {
            float turnInput = playerInput.actions["Move"].ReadValue<Vector2>().x;

            float targetTiltAngle = (isGrounded && Mathf.Abs(turnInput) > 0.1f)
                ? -turnInput * plankTiltAngle
                : 0f;

            if (plank)
            {
                plank.localRotation = Quaternion.Lerp(
                    plank.localRotation,
                    Quaternion.Euler(0f, 0f, targetTiltAngle),
                    Time.deltaTime * hingeRotationSpeed
                );
            }
        }

        /// <summary>
        /// Spins the front wheel based on forward velocity.
        /// </summary>
        private void RotateWheels()
        {
            if (!frontWheel) return;

            float speed = rb.velocity.magnitude;
            if (speed > 0.1f)
            {
                float wheelRotation = speed * wheelRotationSpeed * Time.deltaTime;
                frontWheel.Rotate(Vector3.right, wheelRotation);
            }
        }

        /// <summary>
        /// Zero out velocity if you want to reset the board instantaneously.
        /// </summary>
        public void ResetSpeed()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}







