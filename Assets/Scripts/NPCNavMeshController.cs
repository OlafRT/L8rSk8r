using UnityEngine;
using UnityEngine.AI;

// Ensure both a NavMeshAgent and an AudioSource are attached to the NPC.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
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
    [Tooltip("Animator with parameters: IsWalking, IsRunning, Idle, Attack, etc.")]
    public Animator animator;

    [Header("Sound Settings")]
    [Tooltip("Array of sounds to play randomly when the NPC starts chasing the player.")]
    public AudioClip[] chaseSounds;

    [Header("Attack Settings")]
    [Tooltip("Attack cooldown (in seconds).")]
    public float attackCooldown = 3f;
    // Declare attackTimer so it exists in the context.
    private float attackTimer = 0f;
    [Tooltip("Trigger name for Attack 1.")]
    public string attack1Trigger = "Attack1";
    [Tooltip("Trigger name for Attack 2.")]
    public string attack2Trigger = "Attack2";
    [Tooltip("Trigger name for Attack 3.")]
    public string attack3Trigger = "Attack3";
    [Tooltip("Audio clip for Attack 1.")]
    public AudioClip attack1Sound;
    [Tooltip("Audio clip for Attack 2.")]
    public AudioClip attack2Sound;
    [Tooltip("Audio clip for Attack 3.")]
    public AudioClip attack3Sound;

    // ----- Private Variables -----
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private float idleTimer;

    // Define the NPC states.
    private enum NPCState { Idle, Wander, Chase, Attack, Death }
    private NPCState currentState = NPCState.Wander;
    private NPCState previousState = NPCState.Wander;

    // --- NEW: AudioSource & flag for one-time chase sound ---
    private AudioSource audioSource;
    private bool chaseSoundPlayed = false;  // Ensures the chase sound is played only once

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

        // --- NEW: Get the AudioSource component from the NPC ---
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // If the enemy is dead, do nothing.
        if (currentState == NPCState.Death)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

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

        // --- NEW: Play the chase sound only once when entering the Chase state ---
        if (currentState == NPCState.Chase && !chaseSoundPlayed)
        {
            PlayChaseSound();
        }

        previousState = currentState;
    }

    // --------------------
    // State Methods
    // --------------------
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

    void ChaseUpdate()
    {
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsWalking", false);
        animator.SetBool("Idle", false);

        if (player != null)
        {
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            {
                currentState = NPCState.Attack;
                agent.isStopped = true;
            }
            else if (Vector3.Distance(transform.position, player.position) > playerDetectionDistance)
            {
                currentState = NPCState.Wander;
                agent.speed = wanderSpeed;
                SetNewWanderDestination();
            }
        }
    }

    void AttackUpdate()
    {
        // Stop the agent while attacking.
        agent.isStopped = true;
        attackTimer = attackCooldown;

        // Randomly choose one of three attacks.
        int attackChoice = Random.Range(0, 3);
        switch (attackChoice)
        {
            case 0:
                animator.SetTrigger(attack1Trigger);
                PlaySound(attack1Sound);
                break;
            case 1:
                animator.SetTrigger(attack2Trigger);
                PlaySound(attack2Sound);
                break;
            case 2:
                animator.SetTrigger(attack3Trigger);
                PlaySound(attack3Sound);
                break;
        }

        // After the attack, resume chasing.
        currentState = NPCState.Chase;
        agent.isStopped = false;
    }

    // --------------------
    // Helper Methods
    // --------------------
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

    void PlayChaseSound()
    {
        if (chaseSounds != null && chaseSounds.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, chaseSounds.Length);
            audioSource.PlayOneShot(chaseSounds[index]);
            chaseSoundPlayed = true;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}



