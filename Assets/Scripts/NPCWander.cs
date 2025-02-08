using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWander : MonoBehaviour
{
    [Tooltip("The speed at which the NPC moves.")]
    public float speed = 3.0f;

    [Tooltip("The speed at which the NPC moves towards the player.")]
    public float chaseSpeed = 5.0f;

    [Tooltip("The distance within which the NPC will try to avoid obstacles.")]
    public float obstacleAvoidanceDistance = 5.0f;

    [Tooltip("The distance within which the NPC will detect the player.")]
    public float playerDetectionDistance = 10.0f;

    [Tooltip("The distance within which the NPC will attack the player.")]
    public float attackDistance = 1.5f;

    [Tooltip("Enable or disable moving towards the player.")]
    public bool moveToPlayer = false;

    [Tooltip("The time interval in seconds to set a new wander target.")]
    public float wanderInterval = 5.0f;

    private Transform player;
    private Animator animator;
    private Vector3 wanderTarget;
    private bool isWandering = true;
    private float wanderTimer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        SetNewWanderTarget();
        wanderTimer = wanderInterval;
    }

    private void Update()
    {
        wanderTimer -= Time.deltaTime;

        if (moveToPlayer && Vector3.Distance(transform.position, player.position) <= playerDetectionDistance)
        {
            if (IsPathClear(player.position))
            {
                MoveTowards(player.position, chaseSpeed);
                if (Vector3.Distance(transform.position, player.position) <= attackDistance)
                {
                    animator.SetTrigger("Attack");
                }
                else
                {
                    animator.SetBool("IsWalking", true);
                }
            }
            else
            {
                SetNewWanderTarget();
            }
        }
        else
        {
            Wander();
        }

        // Ensure the NPC stays on the ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    private void Wander()
    {
        if (isWandering)
        {
            MoveTowards(wanderTarget, speed);

            if (Vector3.Distance(transform.position, wanderTarget) < 1.0f || wanderTimer <= 0)
            {
                SetNewWanderTarget();
                wanderTimer = wanderInterval;
            }
        }
    }

    private void MoveTowards(Vector3 target, float moveSpeed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Ensure the NPC only moves on the horizontal plane

        // Avoid obstacles dynamically
        Vector3 avoidance = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, obstacleAvoidanceDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                avoidance = hit.normal * obstacleAvoidanceDistance;
            }
        }

        Vector3 moveDirection = direction + avoidance;
        moveDirection.Normalize();

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * moveSpeed);

        animator.SetBool("IsWalking", true);
    }

    private void SetNewWanderTarget()
    {
        float wanderRadius = 10.0f;
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = 0; // Ensure the target is on the ground

        wanderTarget = randomDirection;
        isWandering = true;
    }

    private bool IsPathClear(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Ensure the NPC only checks on the horizontal plane

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, Vector3.Distance(transform.position, target)))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                return false;
            }
        }
        return true;
    }
}