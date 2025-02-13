using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class NPCDeathController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the NPC. Number of hits required to kill it.")]
    public int maxHealth = 5;  // Change this value if you need more hits before dying.
    private int currentHealth;

    [Header("Hit Effects")]
    [Tooltip("Particle effect to play each time the NPC is hit (while still alive).")]
    public ParticleSystem hitEffect;

    [Header("Death Effects")]
    [Tooltip("Particle effects to play when the NPC dies (on the final hit).")]
    public ParticleSystem[] deathEffects;

    [Header("Animation Settings")]
    [Tooltip("Animator controlling the NPC's animations.")]
    public Animator animator;
    [Tooltip("Trigger name for the hit reaction animation.")]
    public string hitAnimationTrigger = "Hit";
    [Tooltip("Trigger name for the death animation.")]
    public string deathAnimationTrigger = "Death";

    [Tooltip("Bool parameter in the Animator to lock the NPC in the death state.")]
    public string deadBoolName = "Dead";

    [Header("Destruction Settings")]
    [Tooltip("Time (in seconds) after death before the NPC is destroyed.")]
    public float destroyDelay = 15f;

    [Header("Weapon Collision Settings")]
    [Tooltip("The layer mask for the weapon layer.")]
    public LayerMask weaponLayer;
    [Tooltip("Animator bool to check if the attacking object is currently attacking.")]
    public string attackingBoolName = "IsAttacking";

    [Header("Sound Settings")]
    [Tooltip("Array of hit sounds to play at random when the NPC is hit.")]
    public AudioClip[] hitSounds;
    [Tooltip("Sound to play when the NPC dies.")]
    public AudioClip deathSound;

    [Header("Grounding Settings")]
    [Tooltip("Additional Y offset to adjust the NPC's final death position relative to the ground.")]
    public float groundOffset = 0f;

    // Internal flag to prevent multiple death triggers.
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Called when another collider hits this NPC's collider.
    /// Checks if the collider is on the weapon layer and if its root object is attacking.
    /// If so, applies one unit of damage.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 1) Check if the colliding object is on the designated weapon layer.
        if (!IsOnWeaponLayer(collision.gameObject))
            return;

        // 2) Check if the colliding object's root has an Animator with its attacking bool set to true.
        if (!IsAttacking(collision.gameObject))
            return;

        // 3) Conditions met: apply one unit of damage.
        TakeDamage(1);
    }

    /// <summary>
    /// Checks if the given GameObject is on one of the layers defined in weaponLayer.
    /// </summary>
    private bool IsOnWeaponLayer(GameObject obj)
    {
        return (weaponLayer.value & (1 << obj.layer)) != 0;
    }

    /// <summary>
    /// Returns true if the root object of the given GameObject has an Animator whose
    /// 'attackingBoolName' parameter is currently true.
    /// </summary>
    private bool IsAttacking(GameObject obj)
    {
        Animator anim = obj.transform.root.GetComponentInChildren<Animator>();
        if (anim == null)
            return false;
        return anim.GetBool(attackingBoolName);
    }

    /// <summary>
    /// Applies damage to the NPC. If still alive, plays the hit animation, particle effect, and a random hit sound.
    /// On the final hit, plays death particle effects and triggers the death sequence.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            // Play hit reaction animation.
            if (animator != null)
                animator.SetTrigger(hitAnimationTrigger);

            // Play the hit particle effect.
            if (hitEffect != null)
                hitEffect.Play();

            // Play a random hit sound.
            if (hitSounds != null && hitSounds.Length > 0)
            {
                int index = Random.Range(0, hitSounds.Length);
                AudioSource.PlayClipAtPoint(hitSounds[index], transform.position);
            }
        }
        else
        {
            // On the final hit, play all death particle effects.
            if (deathEffects != null)
            {
                foreach (ParticleSystem ps in deathEffects)
                {
                    if (ps != null)
                        ps.Play();
                }
            }
            Die();
        }
    }

    /// <summary>
    /// Handles the NPC's death: triggers the death animation and sound,
    /// disables movement/AI and collisions so the corpse doesn't slide,
    /// forces the NPC to align with the ground (plus an adjustable offset),
    /// and schedules the destruction of the NPC.
    /// </summary>
    private void Die()
    {
        if (isDead)
            return;
        isDead = true;

        // Set the 'Dead' bool on the animator, so transitions can't exit the death state.
        if (animator != null)
        {
            animator.SetBool(deadBoolName, true);
            animator.SetTrigger(deathAnimationTrigger);
        }

        // Play the death sound.
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Disable the NavMeshAgent so it stops moving.
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        // Optionally disable any custom NPC movement/AI scripts.
        NPCNavMeshController npcController = GetComponent<NPCNavMeshController>();
        if (npcController != null)
            npcController.enabled = false;

        // Disable the collider so further collisions don't register.
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Enable the Rigidbody to let physics take over (if not already active).
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Snap the NPC to the ground with an adjustable offset.
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 100f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
        }

        // Freeze the NPC so it doesn't continue falling.
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Finally, destroy the NPC GameObject after the specified delay.
        Destroy(gameObject, destroyDelay);
    }
}






