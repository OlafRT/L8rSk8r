using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class BossController : MonoBehaviour
{
    //===================================================
    // STATES & TIMING
    //===================================================
    private enum BossState { Idle, Speech, Chase, Attack, Death }
    private BossState currentState = BossState.Idle;
    
    [Header("Detection & Movement")]
    [Tooltip("Distance at which the boss detects the player.")]
    public float detectionDistance = 20f;
    [Tooltip("Distance from the player at which the boss starts its attack.")]
    public float attackDistance = 3f;
    [Tooltip("Speed when chasing (walking speed – not running!).")]
    public float walkSpeed = 2f;
    [Tooltip("How quickly the boss rotates to face its movement direction.")]
    public float turnSpeed = 5f;
    [Tooltip("If true, the boss will always rotate to face the player, regardless of movement.")]
    public bool alwaysLookAtPlayer = false;
    
    [Header("Speech Settings")]
    [Tooltip("Duration of the speech (in seconds) before chasing.")]
    public float speechDuration = 40f;
    [Tooltip("Trigger name for the speech animation.")]
    public string speechAnimationTrigger = "Speech";
    [Tooltip("Optional: Audio clip to play during the speech.")]
    public AudioClip speechAudioClip;
    
    [Header("Attack Settings")]
    [Tooltip("Cooldown (in seconds) between attacks.")]
    public float attackCooldown = 3f;
    private float attackTimer = 0f;
    
    [Header("Attack Animation Triggers")]
    public string swingAttackTrigger = "SwingAttack";
    public string kickAttackTrigger = "KickAttack";
    public string comboAttackTrigger = "ComboAttack";
    public string leapAttackTrigger = "LeapAttack";
    
    [Header("Leap Attack Settings")]
    [Tooltip("How far (in world units) the boss will leap when performing a leap attack.")]
    public float leapAttackDistance = 10f;
    [Tooltip("Duration (in seconds) over which the boss leaps smoothly.")]
    public float leapDuration = 1f;
    [Tooltip("The peak height of the leap arc.")]
    public float leapHeight = 5f;
    
    [Header("Attack Sound Effects")]
    public AudioClip swingAttackSound;
    public AudioClip kickAttackSound;
    public AudioClip comboAttackSound;
    public AudioClip leapAttackSound;
    
    [Header("Hit Reaction")]
    [Tooltip("Array of hit reaction animation triggers.")]
    public string[] hitReactionTriggers;
    [Tooltip("Particle effect to play each time the boss is hit (while still alive).")]
    public ParticleSystem hitEffect;
    [Tooltip("Array of hit/reaction sounds to play when the boss is hit.")]
    public AudioClip[] hitSounds;
    
    [Header("Phase Voice Lines")]
    [Tooltip("Voice line to play when health drops below 80% (played once).")]
    public AudioClip phase80Sound;
    [Tooltip("Voice line to play when health drops below 70% (played once).")]
    public AudioClip phase70Sound;
    [Tooltip("Voice line to play when health drops below 50% (played once).")]
    public AudioClip phase50Sound;
    
    [Header("Phase Visuals")]
    [Tooltip("GameObject to enable when health drops below 80% (played once).")]
    public GameObject phase80Object;
    [Tooltip("GameObject to enable when health drops below 70% (played once).")]
    public GameObject phase70Object;
    [Tooltip("GameObject to enable when health drops below 50% (played once).")]
    public GameObject phase50Object;
    [Tooltip("GameObject to enable when health drops below 10% (played once).")]
    public GameObject phase10Object;
    [Tooltip("Voice line to play when health drops below 10% (played once).")]
    public AudioClip phase10Sound;
    
    [Header("Death Effects")]
    [Tooltip("Particle effects to play when the boss dies (on the final hit).")]
    public ParticleSystem[] deathEffects;
    
    [Header("Weapon Collision Settings")]
    [Tooltip("The layer mask for the weapon layer.")]
    public LayerMask weaponLayer;
    [Tooltip("Reference to the Animator on the attacking object (the one that holds the IsAttacking bool).")]
    public Animator attackerAnimator;  // ← Assign this in the Inspector!
    [Tooltip("Animator bool to check if the attacking object is currently attacking.")]
    public string attackingBoolName = "IsAttacking";
    
    [Header("Destruction Settings")]
    [Tooltip("Time (in seconds) after death before the boss is destroyed.")]
    public float destroyDelay = 15f;
    
    [Header("Grounding Settings")]
    [Tooltip("Additional Y offset to adjust the boss's final death position relative to the ground.")]
    public float groundOffset = 0f;
    
    [Header("Hit & Death")]
    [Tooltip("Maximum health of the boss.")]
    public int maxHealth = 100;
    [Tooltip("Audio clip to play when the boss dies.")]
    public AudioClip deathSound;
    [Tooltip("Trigger name for the death animation.")]
    public string deathAnimationTrigger = "Death";
    
    [Header("Idle Animation")]
    [Tooltip("Name of the idle bool parameter.")]
    public string idleBoolParameter = "Idle";
    
    [Header("UI Health Bar")]
    [Tooltip("Assign a UI Slider to display the boss's health.")]
    public Slider healthBar;
    [Tooltip("Assign the parent UI panel for the boss health (will be hidden upon death).")]
    public GameObject bossHealthUIPanel;
    
    [Header("Music Settings")]
    [Tooltip("Reference to the Music Manager (to switch music when combat starts/ends).")]
    public MusicManager musicManager;
    
    //===================================================
    // COMPONENTS & INTERNAL
    //===================================================
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private Transform player;
    private int currentHealth;
    
    // Flags for speech immunity and phase voice lines.
    private bool hasSpoken = false;
    private bool isSpeaking = false;
    private bool isImmune = false;
    
    private bool phase80Played = false;
    private bool phase70Played = false;
    private bool phase50Played = false;
    private bool phase10Played = false;
    
    // Flag to ensure we only trigger combat music once.
    private bool combatMusicStarted = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        agent.speed = walkSpeed;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        currentState = BossState.Idle;
    }
    
    void Update()
    {
        if (currentState == BossState.Death)
            return;
        
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
        
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= detectionDistance)
            {
                if (!hasSpoken && !isSpeaking)
                {
                    currentState = BossState.Speech;
                    isSpeaking = true;
                    isImmune = true;  // Boss is immune during his speech.
                    StartCoroutine(PerformSpeech());
                }
                else if (hasSpoken)
                {
                    if (distanceToPlayer > attackDistance)
                    {
                        currentState = BossState.Chase;
                        agent.isStopped = false;
                        agent.SetDestination(player.position);
                        
                        // Trigger combat music once when chasing starts.
                        if (!combatMusicStarted && musicManager != null)
                        {
                            musicManager.EnterCombat();
                            combatMusicStarted = true;
                        }
                    }
                    else if (attackTimer <= 0f)
                    {
                        currentState = BossState.Attack;
                        agent.isStopped = true;
                        PerformAttack();
                    }
                }
            }
            else
            {
                currentState = BossState.Idle;
                agent.isStopped = true;
            }
        }
        
        if (currentState == BossState.Chase && player != null)
            agent.SetDestination(player.position);
        
        // Rotation: if alwaysLookAtPlayer is enabled, always rotate to face the player;
        // otherwise, use movement direction.
        if (alwaysLookAtPlayer && player != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        
        animator.SetBool("IsWalking", agent.velocity.sqrMagnitude > 0.1f);
        animator.SetBool(idleBoolParameter, currentState == BossState.Idle);
    }
    
    //===============================
    // SPEECH BEHAVIOR
    //===============================
    IEnumerator PerformSpeech()
    {
        agent.isStopped = true;
        animator.SetTrigger(speechAnimationTrigger);
        if (speechAudioClip != null)
        {
            audioSource.clip = speechAudioClip;
            audioSource.Play();
        }
        yield return new WaitForSeconds(speechDuration);
        hasSpoken = true;
        isSpeaking = false;
        isImmune = false; // End immunity after speech.
        agent.isStopped = false;
    }
    
    //===============================
    // ATTACK BEHAVIOR
    //===============================
    void PerformAttack()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        attackTimer = attackCooldown;
        
        // Check phase thresholds and play voice lines and enable corresponding objects (only once).
        if (currentHealth > 0)
        {
            if (healthPercent <= 0.8f && !phase80Played)
            {
                PlaySound(phase80Sound);
                if (phase80Object != null) phase80Object.SetActive(true);
                phase80Played = true;
            }
            if (healthPercent <= 0.7f && !phase70Played)
            {
                PlaySound(phase70Sound);
                if (phase70Object != null) phase70Object.SetActive(true);
                phase70Played = true;
            }
            if (healthPercent <= 0.5f && !phase50Played)
            {
                PlaySound(phase50Sound);
                if (phase50Object != null) phase50Object.SetActive(true);
                phase50Played = true;
            }
            if (healthPercent <= 0.1f && !phase10Played)
            {
                PlaySound(phase10Sound);
                if (phase10Object != null) phase10Object.SetActive(true);
                phase10Played = true;
            }
        }
        
        // Choose an attack based on current health percentage.
        if (healthPercent > 0.8f)
        {
            animator.SetTrigger(swingAttackTrigger);
            PlaySound(swingAttackSound);
        }
        else if (healthPercent > 0.7f)
        {
            if (Random.value < 0.5f)
            {
                animator.SetTrigger(swingAttackTrigger);
                PlaySound(swingAttackSound);
            }
            else
            {
                animator.SetTrigger(kickAttackTrigger);
                PlaySound(kickAttackSound);
            }
        }
        else if (healthPercent > 0.5f)
        {
            float rand = Random.value;
            if (rand < 0.33f)
            {
                animator.SetTrigger(swingAttackTrigger);
                PlaySound(swingAttackSound);
            }
            else if (rand < 0.66f)
            {
                animator.SetTrigger(kickAttackTrigger);
                PlaySound(kickAttackSound);
            }
            else
            {
                animator.SetTrigger(comboAttackTrigger);
                PlaySound(comboAttackSound);
            }
        }
        else
        {
            float rand = Random.value;
            if (rand < 0.25f)
            {
                animator.SetTrigger(swingAttackTrigger);
                PlaySound(swingAttackSound);
            }
            else if (rand < 0.5f)
            {
                animator.SetTrigger(kickAttackTrigger);
                PlaySound(kickAttackSound);
            }
            else if (rand < 0.75f)
            {
                animator.SetTrigger(comboAttackTrigger);
                PlaySound(comboAttackSound);
            }
            else
            {
                StartCoroutine(PerformLeapAttack());
                return; // The leap coroutine handles its own cooldown.
            }
        }
    }
    
    //===============================
    // LEAP ATTACK (Smooth Parabolic Arc)
    //===============================
    IEnumerator PerformLeapAttack()
    {
        animator.SetTrigger(leapAttackTrigger);
        PlaySound(leapAttackSound);
        agent.isStopped = true;
        
        Vector3 startPos = transform.position;
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 endPos = transform.position + direction * leapAttackDistance;
        
        float elapsed = 0f;
        while (elapsed < leapDuration)
        {
            float t = elapsed / leapDuration;
            // Lerp horizontally between start and end.
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            // Add a parabolic vertical arc.
            pos.y += 4 * leapHeight * t * (1 - t);
            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        
        agent.isStopped = false;
        attackTimer = attackCooldown;
    }
    
    //===============================
    // COLLISION DAMAGE HANDLING
    //===============================
    private void OnCollisionEnter(Collision collision)
    {
        if (isImmune)
            return;  // Boss is not attackable during speech.
        if (!IsOnWeaponLayer(collision.gameObject))
            return;
        if (!IsAttacking(collision.gameObject))
            return;
        TakeDamage(1);
    }
    
    private bool IsOnWeaponLayer(GameObject obj)
    {
        return (weaponLayer.value & (1 << obj.layer)) != 0;
    }
    
    private bool IsAttacking(GameObject obj)
    {
        if (attackerAnimator != null)
        {
            return attackerAnimator.GetBool(attackingBoolName);
        }
        else
        {
            Animator anim = obj.transform.root.GetComponentInChildren<Animator>();
            if (anim == null)
                return false;
            return anim.GetBool(attackingBoolName);
        }
    }
    
    //===============================
    // DAMAGE & DEATH HANDLING
    //===============================
    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Death || isImmune)
            return;
        
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        
        UpdateHealthBar();
        
        if (currentHealth > 0)
        {
            if (hitReactionTriggers != null && hitReactionTriggers.Length > 0)
            {
                int index = Random.Range(0, hitReactionTriggers.Length);
                animator.SetTrigger(hitReactionTriggers[index]);
            }
            if (hitEffect != null)
                hitEffect.Play();
            if (hitSounds != null && hitSounds.Length > 0)
            {
                int idx = Random.Range(0, hitSounds.Length);
                PlaySound(hitSounds[idx]);
            }
        }
        else
        {
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
    
    void Die()
    {
        currentState = BossState.Death;
        agent.isStopped = true;
        animator.SetTrigger(deathAnimationTrigger);
        PlaySound(deathSound);
        
        if (musicManager != null)
        {
            // Exit combat mode so regular music resumes.
            musicManager.ExitCombat();
        }
        
        if (agent != null)
            agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        
        // Snap the boss to the ground with an offset.
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 100f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
        }
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Force the health bar to zero and hide the panel.
        if (healthBar != null)
            healthBar.value = 0;
        if (bossHealthUIPanel != null)
            bossHealthUIPanel.SetActive(false);
        
        Destroy(gameObject, destroyDelay);
    }
    
    //===============================
    // HELPER METHODS
    //===============================
    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
    
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
}









