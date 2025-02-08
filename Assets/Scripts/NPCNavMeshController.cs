using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCNavMeshController : MonoBehaviour
{
    // ----- Movement & Behavior Settings -----
    [Header("Movement Settings")]
    [Tooltip("Speed when wandering.")]
    public float wanderSpeed = 3f;
    [Tooltip("Speed when chasing the player.")]
    public float chaseSpeed = 5f;
    [Tooltip("How quickly the NPC rotates to face its movement direction.")]
    public float turnSpeed = 5f;
    [Tooltip("Distance at which the NPC detects the player.")]
    public float playerDetectionDistance = 15f;
    [Tooltip("Distance at which the NPC attacks the player.")]
    public float attackDistance = 2f;
    [Tooltip("If true, the NPC will chase the player when within detection range.")]
    public bool moveToPlayer = true;

    [Header("Wander Settings")]
    [Tooltip("Time interval (in seconds) between choosing a new wander destination.")]
    public float wanderInterval = 5f;
    [Tooltip("Radius within which a new wander destination is chosen.")]
    public float wanderRadius = 10f;
    [Tooltip("Time (in seconds) to stand idle when a wander destination is reached.")]
    public float idleTime = 2f;

    [Header("Animation")]
    [Tooltip("Animator with parameters: IsWalking, IsRunning, Idle, Attack.")]
    public Animator animator;

    [Header("Sound Settings")]
    [Tooltip("Array of sounds to play randomly when the NPC starts chasing the player.")]
    public AudioClip[] chaseSounds;

    // ----- Private Variables -----
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private float idleTimer;

    // Define the NPC states.
    private enum NPCState { Idle, Wander, Chase, Attack }
    private NPCState currentState = NPCState.Wander;
    private NPCState previousState = NPCState.Wander;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Set the initial movement speed.
        agent.speed = wanderSpeed;

        // Find the player (make sure your Player is tagged "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Set an initial wander destination.
        SetNewWanderDestination();
        wanderTimer = wanderInterval;
        idleTimer = idleTime;

        previousState = currentState;
    }

    private void Update()
    {
        // --------------------
        // State Decision Logic
        // --------------------
        if (moveToPlayer && player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            // If the player is very close, attack.
            if (distToPlayer <= attackDistance)
            {
                currentState = NPCState.Attack;
            }
            // If the player is within detection distance but not too close, chase.
            else if (distToPlayer <= playerDetectionDistance)
            {
                currentState = NPCState.Chase;
            }
            else
            {
                // Otherwise, if not in chase range, wander.
                if (currentState != NPCState.Idle)
                    currentState = NPCState.Wander;
            }
        }
        else
        {
            currentState = NPCState.Wander;
        }

        // --------------------
        // State Behavior
        // --------------------
        switch (currentState)
        {
            case NPCState.Wander:
                WanderUpdate();
                break;
            case NPCState.Idle:
                IdleUpdate();
                break;
            case NPCState.Chase:
                ChaseUpdate();
                break;
            case NPCState.Attack:
                AttackUpdate();
                break;
        }

        // Optional: Face movement direction.
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // If we have just transitioned into Chase state, play a random chase sound.
        if (currentState == NPCState.Chase && previousState != NPCState.Chase)
        {
            PlayChaseSound();
        }
        previousState = currentState;
    }

    // --------------------
    // State Methods
    // --------------------

    // Wander: Move toward a randomly chosen destination.
    void WanderUpdate()
    {
        // Set animator parameters.
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);

        // If the agent has reached its destination...
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Switch to Idle state.
            currentState = NPCState.Idle;
            idleTimer = idleTime;
            animator.SetBool("Idle", true);
            animator.SetBool("IsWalking", false);
        }
    }

    // Idle: Stand still for a moment before wandering again.
    void IdleUpdate()
    {
        // Remain idle for idleTimer seconds.
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            // Switch back to Wander state.
            currentState = NPCState.Wander;
            SetNewWanderDestination();
            agent.speed = wanderSpeed;
            animator.SetBool("Idle", false);
            animator.SetBool("IsWalking", true);
        }

        // Also, if the player appears during idle, switch to chase.
        if (moveToPlayer && player != null && Vector3.Distance(transform.position, player.position) <= playerDetectionDistance)
        {
            currentState = NPCState.Chase;
            agent.speed = chaseSpeed;
            animator.SetBool("Idle", false);
            animator.SetBool("IsRunning", true);
        }
    }

    // Chase: Set the agent's destination to the player's position.
    void ChaseUpdate()
    {
        // Set animator parameters.
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsWalking", false);
        animator.SetBool("Idle", false);

        if (player != null)
        {
            agent.SetDestination(player.position);

            // If the player is within attack distance, switch to attack state.
            if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            {
                currentState = NPCState.Attack;
                agent.isStopped = true;
            }
            // If the player escapes detection, revert to wander.
            else if (Vector3.Distance(transform.position, player.position) > playerDetectionDistance)
            {
                currentState = NPCState.Wander;
                agent.speed = wanderSpeed;
                SetNewWanderDestination();
            }
        }
    }

    // Attack: Trigger the attack animation and then resume chasing.
    void AttackUpdate()
    {
        // Stop the agent while attacking.
        agent.isStopped = true;
        animator.SetTrigger("Attack");

        // After the attack (or after a short delay), resume chasing.
        // For simplicity, we immediately switch back to chase.
        currentState = NPCState.Chase;
        agent.isStopped = false;
    }

    // --------------------
    // Helper Methods
    // --------------------

    // Choose a new random destination within wanderRadius.
    void SetNewWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    // Plays a random chase sound from the chaseSounds array.
    void PlayChaseSound()
    {
        if (chaseSounds != null && chaseSounds.Length > 0)
        {
            int index = Random.Range(0, chaseSounds.Length);
            AudioSource.PlayClipAtPoint(chaseSounds[index], transform.position);
        }
    }
}


