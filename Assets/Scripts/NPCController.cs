using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class NPCController : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("If true, this NPC chases and attacks the player; if false, it just wanders.")]
    public bool isEvil = true;

    [Header("Detection & Attack Ranges")]
    [Tooltip("Distance within which the NPC will start chasing the player.")]
    public float detectionRange = 20f;
    [Tooltip("Distance within which the NPC will attack the player.")]
    public float attackRange = 2f;

    [Header("Movement Speeds")]
    [Tooltip("Speed when wandering.")]
    public float wanderSpeed = 3f;
    [Tooltip("Speed when chasing the player.")]
    public float chaseSpeed = 5f;

    [Header("Wander Settings")]
    [Tooltip("Radius within which a new wander destination is chosen.")]
    public float wanderRadius = 10f;
    [Tooltip("Time (in seconds) between wander destination changes.")]
    public float wanderInterval = 5f;

    [Header("Attack Settings")]
    [Tooltip("Time (in seconds) between attacks.")]
    public float attackCooldown = 1.5f;
    [Tooltip("Trigger name for Attack 1.")]
    public string attack1Trigger = "Attack1";
    [Tooltip("Trigger name for Attack 2.")]
    public string attack2Trigger = "Attack2";
    [Tooltip("Trigger name for Attack 3.")]
    public string attack3Trigger = "Attack3";
    [Tooltip("Sound for Attack 1.")]
    public AudioClip attack1Sound;
    [Tooltip("Sound for Attack 2.")]
    public AudioClip attack2Sound;
    [Tooltip("Sound for Attack 3.")]
    public AudioClip attack3Sound;

    // Components
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Transform player;

    // State Machine
    private enum NPCState { Wander, Chase, Attack }
    private NPCState currentState = NPCState.Wander;
    private bool isAttacking = false;

    // Wander management
    private Vector3 wanderTarget;
    private float wanderTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Try to find the player (make sure your player is tagged "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        wanderTimer = wanderInterval;
        SetNewWanderTarget();
    }

    void Update()
    {
        // If not evil, just wander.
        if (!isEvil || player == null)
        {
            currentState = NPCState.Wander;
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > detectionRange)
            {
                currentState = NPCState.Wander;
            }
            else if (distanceToPlayer > attackRange)
            {
                currentState = NPCState.Chase;
            }
            else
            {
                currentState = NPCState.Attack;
            }
        }

        // Execute behavior based on state.
        switch (currentState)
        {
            case NPCState.Wander:
                Wander();
                break;
            case NPCState.Chase:
                Chase();
                break;
            case NPCState.Attack:
                Attack();
                break;
        }

        // Update movement-based animations.
        UpdateAnimations();

        // Smoothly rotate toward movement direction if moving.
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Wanders by selecting a random point on the NavMesh at intervals.
    /// </summary>
    void Wander()
    {
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f || agent.remainingDistance < 0.5f)
        {
            SetNewWanderTarget();
            wanderTimer = wanderInterval;
        }
        agent.SetDestination(wanderTarget);
    }

    /// <summary>
    /// Chases the player by setting the destination to the player's position.
    /// </summary>
    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    /// <summary>
    /// Attacks the player if within attack range. Stops moving and launches an attack.
    /// </summary>
    void Attack()
    {
        // Stop movement so we don’t just push the player.
        agent.isStopped = true;
        // Face the player.
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    /// <summary>
    /// Waits for the attack cooldown before allowing another attack.
    /// Randomly selects one of three attack animations and plays its sound.
    /// </summary>
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        int attackChoice = Random.Range(0, 3);
        switch (attackChoice)
        {
            case 0:
                animator.SetTrigger(attack1Trigger);
                if (attack1Sound != null) audioSource.PlayOneShot(attack1Sound);
                break;
            case 1:
                animator.SetTrigger(attack2Trigger);
                if (attack2Sound != null) audioSource.PlayOneShot(attack2Sound);
                break;
            case 2:
                animator.SetTrigger(attack3Trigger);
                if (attack3Sound != null) audioSource.PlayOneShot(attack3Sound);
                break;
        }
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        // Resume chasing immediately after the attack.
        agent.isStopped = false;
    }

    /// <summary>
    /// Selects a random point on the NavMesh within wanderRadius.
    /// </summary>
    void SetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            wanderTarget = hit.position;
        else
            wanderTarget = transform.position;
    }

    /// <summary>
    /// Sets animation parameters based on agent velocity and state.
    /// </summary>
    void UpdateAnimations()
    {
        float speed = agent.velocity.magnitude;
        if (currentState == NPCState.Chase)
        {
            animator.SetBool("IsRunning", speed > 0.1f);
            animator.SetBool("IsWalking", false);
        }
        else if (currentState == NPCState.Wander)
        {
            animator.SetBool("IsWalking", speed > 0.1f);
            animator.SetBool("IsRunning", false);
        }
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }
    }

    // Optional: Visualize detection and attack ranges in the editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}




