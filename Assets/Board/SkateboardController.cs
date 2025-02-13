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
        // (Assumes additional animator parameters: "Ollie", "TurnLeft", "TurnRight", "Grounded", "InAir", and "Land")

        [Header("Ground Alignment")]
        [Tooltip("Speed for slerp alignment to ground when grounded.")]
        public float groundAlignSpeed = 10f; 
        public float timeSinceGrounded = 0f;
        public float groundedCooldown = 0.1f; // Delay before aligning stops after landing

        [Header("Lateral Friction (No-Drift)")]
        [Tooltip("0 = remove all sideways velocity each frame; 1 = remove none.")]
        public float lateralVelocityRetention = 0.0f; 

        [Header("Manual Air Rotation")]
        public float rotationSensitivityX = 55f; // Controls rotation strength on X-axis (pitch)
        public float rotationSensitivityY = 200f; // Controls rotation strength on Y-axis (yaw)     

        [Header("Jump Settings")]
        public float jumpForce = 10f; // Adjustable jump force

        [Header("Obstacle Handling")]
        public float bumpTolerance = 0.1f; // Maximum height of obstacles to ride over 

        // Private references
        private Rigidbody rb;
        private PlayerInput playerInput;
        private StarterAssetsInputs _input;

        // Ground check variables
        public bool isGrounded;
        private RaycastHit groundHit;

        // For turning animation (to avoid repeatedly triggering the same animation)
        private bool turningAnimationTriggered = false;

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
            // Visual-only or input-based updates
            AnimateBackHinge();
            TiltPlank();
            RotateWheels();
        }

        private void FixedUpdate()
        {
            // 1) Ground check via raycast
            bool wasGrounded = isGrounded;
            isGrounded = Physics.Raycast(
                transform.position + Vector3.up * bumpTolerance, // start slightly above the board
                Vector3.down,
                out groundHit,
                suspensionRayLength + bumpTolerance, // extend ray length a bit
                groundLayer
            );

            // === NEW: Update Animator Grounded/InAir and Landing states ===
            if (animator != null)
            {
                if (isGrounded)
                {
                    animator.SetBool("Grounded", true);
                    animator.SetBool("InAir", false);
                    if (!wasGrounded)
                    {
                        // Just landed – trigger the landing animation
                        animator.SetTrigger("Land");
                    }
                }
                else
                {
                    animator.SetBool("Grounded", false);
                    animator.SetBool("InAir", true);
                }
            }
            // ================================================================

            // Track how long the board has been in the air
            if (isGrounded)
            {
                timeSinceGrounded = 0f; // reset timer when grounded
            }
            else
            {
                timeSinceGrounded += Time.fixedDeltaTime; // increment timer when airborne
            }

            // 2) Apply custom gravity
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

            // 3) When grounded, use suspension and align to ground
            if (isGrounded)
            {
                ApplySuspension();
                rb.drag = dragOnGround;

                // Align the board’s "up" direction to match the ground normal
                AlignToGroundNormal(groundHit.normal);

                // Optionally remove sideways drift
                ApplyNoDrift();
            }
            else
            {
                // When in air, use lower drag and (after a short delay) allow manual rotation
                rb.drag = dragInAir;
                if (timeSinceGrounded > groundedCooldown)
                {
                    HandleAirRotation();
                }
            }

            // Handle skateboard controls
            HandlePush();
            HandleJump(); 
            HandleBraking();
            HandleTurning();

            // === NEW: Handle Turn Animation Triggers ===
            HandleTurnAnimation();

            LimitSpeed();
        }

        /// <summary>
        /// Applies a spring force to keep the board near the ground.
        /// </summary>
        private void ApplySuspension()
        {
            float distance = groundHit.distance - rideHeightAdjustment;
            if (distance < 0f) distance = 0f;

            float springOffset = suspensionRayLength - distance;
            if (springOffset < 0f) return;

            float springForce = Mathf.Clamp(springOffset * suspensionStrength, 0, suspensionStrength * 0.75f);
            float verticalVelocity = Vector3.Dot(rb.velocity, transform.up);
            float damping = Mathf.Clamp(suspensionDamp * verticalVelocity, -suspensionStrength, suspensionStrength);

            Vector3 force = transform.up * (springForce - damping);
            rb.AddForce(force, ForceMode.Force);

            // Prevent sudden downward velocity spikes
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Max(rb.velocity.y, -1f), rb.velocity.z);
        }

        /// <summary>
        /// Aligns the board’s up-axis with the ground normal.
        /// </summary>
        private void AlignToGroundNormal(Vector3 groundNormal)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * rb.rotation;
            Quaternion finalRotation = Quaternion.Slerp(rb.rotation, targetRotation, groundAlignSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(finalRotation);
        }

        /// <summary>
        /// Allows manual air rotation based on player input.
        /// </summary>
        private void HandleAirRotation()
        {
            float pitchInput = _input.move.y;
            float yawInput = _input.move.x;

            float pitchRotation = pitchInput * rotationSensitivityX * Time.fixedDeltaTime;
            float yawRotation = yawInput * rotationSensitivityY * Time.fixedDeltaTime;

            rb.MoveRotation(rb.rotation * Quaternion.Euler(pitchRotation, yawRotation, 0f));
        }

        /// <summary>
        /// Removes sideways (lateral) velocity to reduce drift.
        /// </summary>
        private void ApplyNoDrift()
        {
            Vector3 velocity = rb.velocity;
            if (velocity.sqrMagnitude < 0.0001f) return;

            Vector3 forwardDir = transform.forward;
            Vector3 forwardComponent = Vector3.Project(velocity, forwardDir);
            Vector3 sidewaysComponent = velocity - forwardComponent;

            Vector3 newVelocity = forwardComponent + sidewaysComponent * lateralVelocityRetention;
            rb.velocity = newVelocity;
        }

        /// <summary>
        /// Handles the push input.
        /// </summary>
        private void HandlePush()
        {
            if (_input.attack && isGrounded)
            {
                rb.AddForce(transform.forward * pushForce, ForceMode.VelocityChange);
                animator?.SetTrigger(pushAnimationTrigger);
                _input.attack = false;
            }
        }

        /// <summary>
        /// Applies braking force when the player presses down.
        /// </summary>
        private void HandleBraking()
        {
            float moveInput = playerInput.actions["Move"].ReadValue<Vector2>().y;
            if (isGrounded && moveInput < 0f && rb.velocity.magnitude > 0.1f)
            {
                Vector3 brakeDirection = -rb.velocity.normalized;
                rb.AddForce(brakeDirection * brakeForce, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Handles turning (rotating the board) when grounded.
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
        /// Handles jump input – applies an upward impulse and triggers the jump ("Ollie") animation.
        /// </summary>
        private void HandleJump()
        {
            if (_input.jump && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator?.SetTrigger("Ollie");
                _input.jump = false;
            }
        }

        /// <summary>
        /// Clamps the skateboard’s speed to the maximum allowed.
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

        /// <summary>
        /// Continuously updates the animator booleans for turning left or right.
        /// When the player is turning, the corresponding boolean is set to true, 
        /// and when there’s no input (or the board is in the air) both are false.
        /// </summary>
        private void HandleTurnAnimation()
        {
            float turnInput = playerInput.actions["Move"].ReadValue<Vector2>().x;

            // When not grounded, disable turning animations.
            if (!isGrounded)
            {
                animator?.SetBool("TurnLeft", false);
                animator?.SetBool("TurnRight", false);
                return;
            }

            // Check the horizontal input threshold.
            if (turnInput > 0.1f)
            {
                animator?.SetBool("TurnRight", true);
                animator?.SetBool("TurnLeft", false);
            }
            else if (turnInput < -0.1f)
            {
                animator?.SetBool("TurnLeft", true);
                animator?.SetBool("TurnRight", false);
            }
            else
            {
                // No significant turning input—reset both.
                animator?.SetBool("TurnLeft", false);
                animator?.SetBool("TurnRight", false);
            }
        }
        // ====================================================================

        // ======================= VISUAL EXTRAS =======================

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
        /// Tilts the board’s plank side-to-side when turning.
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
            float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
            if (Mathf.Abs(forwardSpeed) > 0.1f)
            {
                float wheelRotation = forwardSpeed * wheelRotationSpeed * Time.deltaTime;
                frontWheel.Rotate(Vector3.right, wheelRotation);
            }
        }

        /// <summary>
        /// Immediately resets the board’s velocity.
        /// </summary>
        public void ResetSpeed()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}






