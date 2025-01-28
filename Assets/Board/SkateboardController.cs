using UnityEngine;

public class SkateboardController : MonoBehaviour
{
    public float pushForce = 100f; // Force applied per push
    public float maxSpeed = 15f; // Maximum speed
    public float frictionFactor = 0.99f; // Glide friction for smoother rolling
    public float turnSpeed = 50f; // Maximum turn speed
    public float jumpForce = 10f; // Force applied for jumping
    public LayerMask groundLayer; // Layer for ground detection
    public Transform frontWheel; // Front wheel for rotation
    public Transform backHinge; // Back hinge for steering
    public float hingeRotationSpeed = 5f; // Speed of hinge rotation
    public float maxHingeAngle = 30f; // Maximum hinge angle for turning
    public float wheelRotationSpeed = 5f; // Adjust wheel spin speed
    public float boardTurnMultiplier = 1f; // Multiplier for board rotation speed

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 0; // No drag for smooth rolling
        rb.angularDrag = 0.05f; // Minimal angular drag
    }

    void Update()
    {
        HandlePush();
        HandleJump();
        RotateWheels();
        AnimateHinge();
    }

    void FixedUpdate()
    {
        ApplyFriction();
        HandleTurning();
    }

    private void HandlePush()
    {
        if (Input.GetKeyDown(KeyCode.W) && isGrounded) // Push forward
        {
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(transform.forward * pushForce, ForceMode.Impulse);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && isGrounded) // Push backward
        {
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(-transform.forward * pushForce, ForceMode.Impulse);
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // Jump
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ApplyFriction()
    {
        if (isGrounded && rb.velocity.magnitude > 0.1f)
        {
            rb.velocity *= frictionFactor; // Reduce velocity gradually
        }
    }

    private void HandleTurning()
    {
        float turnInput = Input.GetAxis("Horizontal"); // "A" and "D" or arrow keys for turning

        if (isGrounded && rb.velocity.magnitude > 0.1f && Mathf.Abs(turnInput) > 0.1f)
        {
            // Determine direction of movement (forward or backward)
            // Flip the direction logic here to make it work the opposite way
            float direction = Vector3.Dot(rb.velocity, transform.forward) >= 0 ? -1f : 1f;

            // Scale turn speed based on velocity and direction
            float scaledTurnSpeed = turnSpeed * (rb.velocity.magnitude / maxSpeed);

            // Apply the turn multiplier based on the direction of movement
            transform.Rotate(0, direction * turnInput * scaledTurnSpeed * boardTurnMultiplier * Time.deltaTime, 0);

            // Adjust hinge rotation based on movement direction
            float targetHingeAngle = turnInput * maxHingeAngle * direction;
            backHinge.localRotation = Quaternion.Lerp(
                backHinge.localRotation,
                Quaternion.Euler(0, targetHingeAngle, 0),
                Time.deltaTime * hingeRotationSpeed
            );
        }
        else
        {
            // Reset hinge to neutral position when not moving
            backHinge.localRotation = Quaternion.Lerp(
                backHinge.localRotation,
                Quaternion.Euler(0, 0, 0),
                Time.deltaTime * hingeRotationSpeed
            );
        }
    }


    private void AnimateHinge()
    {
        float turnInput = Input.GetAxis("Horizontal");
        if (isGrounded && Mathf.Abs(turnInput) > 0.1f)
        {
            // Rotate the hinge to simulate weight shift
            float targetHingeAngle = turnInput * maxHingeAngle;
            backHinge.localRotation = Quaternion.Lerp(
                backHinge.localRotation,
                Quaternion.Euler(0, targetHingeAngle, 0),
                Time.deltaTime * hingeRotationSpeed
            );
        }
        else
        {
            // Reset hinge to neutral position
            backHinge.localRotation = Quaternion.Lerp(
                backHinge.localRotation,
                Quaternion.Euler(0, 0, 0),
                Time.deltaTime * hingeRotationSpeed
            );
        }
    }

    private void RotateWheels()
    {
        // Rotate front and back wheels based on speed
        float wheelRotation = rb.velocity.magnitude * wheelRotationSpeed * Time.deltaTime;
        if (rb.velocity.magnitude > 0)
        {
            frontWheel.Rotate(Vector3.right, rb.velocity.z >= 0 ? wheelRotation : -wheelRotation);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}







