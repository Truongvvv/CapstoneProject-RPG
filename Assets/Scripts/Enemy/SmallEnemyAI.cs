using UnityEngine;
using UnityEngine.AI;

public class SmallEnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float fleeThreshold = 20f;
    public float fleeDistance = 10f;
    public float health = 100f;

    private float lostSightTimer = 1f;
    private bool isPatrolling = false;
    public float patrolRadius = 5f;
    private bool hasReturned = false;

    private NavMeshAgent agent;
    private Animator animator;

    private enum State { Idle, Chase, Attack, Flee, Return, Patrol }
    private State currentState = State.Idle;

    private float attackCooldown = 1.5f;
    private float attackTimer = 0f;
    private Vector3 initialPosition;

    public LayerMask playerLayer;
    public int expReward = 50; // EXP khi chết
    private Vector3 spawnPoint;
    private Quaternion spawnRotation;
    [Header("Loot Settings")]
    public GameObject[] lootPrefabs;       // Các vật phẩm có thể rơi
    [Range(0f, 1f)]
    public float dropChance = 0.9f;        // Xác suất rơi (90%)
    [Header("Extra Loot")]
    public GameObject healthPotionPrefab;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;

        // Set spawn point để dùng khi hồi sinh
        spawnPoint = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = distanceToPlayer <= detectionRange;

        if (health <= fleeThreshold && canSeePlayer)
        {
            lostSightTimer = 0f;
            isPatrolling = false;
            currentState = State.Flee;
        }
        else if (canSeePlayer)
        {
            lostSightTimer = 0f;
            isPatrolling = false;

            if (distanceToPlayer <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else
        {
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
            {
                lostSightTimer += Time.deltaTime;

                if (lostSightTimer >= 1f)
                {
                    currentState = State.Patrol;
                }
            }
            else
            {
                lostSightTimer = 0f;
                currentState = State.Return;
            }
        }

        UpdateState();
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", false);
                animator.SetBool("isPatrolling", false);
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                animator.SetBool("isPatrolling", false);
                break;

            case State.Attack:
                agent.isStopped = true;
                transform.LookAt(player);
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);
                Debug.Log("Attack Trigger Set");
                animator.SetBool("isPatrolling", false);

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
                    foreach (var hit in hits)
                    {
                        if (hit.transform == player)
                        {
                            hit.GetComponent<PlayerHealth>()?.TakeDamage(10f);
                            break;
                        }
                    }
                    attackTimer = 0f;
                }
                break;

            case State.Flee:
                Vector3 fleeDir = (transform.position - player.position).normalized;
                Vector3 fleeTarget = transform.position + fleeDir * fleeDistance;

                agent.isStopped = false;
                agent.SetDestination(fleeTarget);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;

            case State.Return:
                agent.isStopped = false;
                animator.SetBool("isAttacking", false);
                animator.SetBool("isRunning", true);
                animator.SetBool("isPatrolling", false);

                agent.SetDestination(initialPosition);

                if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
                {
                    currentState = State.Idle;
                }
                break;

            case State.Patrol:
                agent.isStopped = false;
                animator.SetBool("isRunning", false);
                animator.SetBool("isPatrolling", true); // Bật anim đi tuần
                animator.SetBool("isAttacking", false);

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    Vector3 patrolTarget = GetRandomPatrolPoint();
                    agent.SetDestination(patrolTarget);

                    // Sau 2s sẽ quay về chỗ cũ và bắt đầu lại
                    Invoke(nameof(BackToStart), 2f);
                }
                break;
        }
    }
    void BackToStart()
    {
        currentState = State.Return;
        agent.SetDestination(initialPosition);
    }
    Vector3 GetRandomPatrolPoint()
    {
        Vector2 randCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 randPoint = initialPosition + new Vector3(randCircle.x, 0, randCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randPoint, out hit, 2f, NavMesh.AllAreas))
            return hit.position;
        return initialPosition;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            PlayerCombat combat = other.GetComponentInParent<PlayerCombat>();
            if (combat != null)
            {
                int finalDamage = combat.damage;
                TakeDamage(finalDamage);

                if (finalDamage > 20)
                {
                    Debug.Log("Gậy đánh trúng với buff! Gây damage: " + finalDamage);
                }
                else if (finalDamage == 20)
                {
                    Debug.Log("Gậy đánh trúng! Gây damage mặc định: " + finalDamage);
                }
                else
                {
                    Debug.Log("Gậy đánh trúng! Damage bất thường: " + finalDamage);
                }
            }
            else
            {
                Debug.Log("Không tìm thấy PlayerCombat!");
            }
        }
    }

    void TryDropLoot()
    {
        // 1. Rớt random item
        if (lootPrefabs.Length > 0 && Random.value <= dropChance)
        {
            int index = Random.Range(0, lootPrefabs.Length);
            Vector3 offset = new Vector3(0.5f, 0.5f, 0);
            Instantiate(lootPrefabs[index], transform.position + offset, Quaternion.identity);
        }

        // 2. Luôn rớt bình máu
        if (healthPotionPrefab != null)
        {
            Vector3 offset = new Vector3(-0.5f, 0.5f, 0);
            Instantiate(healthPotionPrefab, transform.position + offset, Quaternion.identity);
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");
        agent.isStopped = true;

        // Ẩn collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Ẩn mesh renderer (model biến mất)
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Tắt NavMeshAgent + AI
        agent.enabled = false;
        this.enabled = false;

        TryDropLoot();

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.GainExp(expReward);
        }

        // Hẹn hồi sinh sau 10 giây
        Invoke(nameof(Respawn), 10f);
    }

    void Respawn()
    {
        // Reset stats
        health = 100f;

        // Hiện lại model
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Bật collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Bật NavMeshAgent + AI
        agent.enabled = true;
        this.enabled = true;

        // Đặt về spawn point
        agent.Warp(spawnPoint);
        transform.rotation = spawnRotation;

        // Reset animator về Idle
        animator.Rebind(); // reset toàn bộ animator state
        animator.Update(0f);
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isPatrolling", false);

        currentState = State.Idle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}