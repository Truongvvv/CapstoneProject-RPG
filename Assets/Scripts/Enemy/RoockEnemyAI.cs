using UnityEngine;
using UnityEngine.AI;

public class RoockEnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 12f;
    public float attackRange = 2f;
    public float health = 300f;
    public int expReward = 200;

    private NavMeshAgent agent;
    private Animator animator;

    private float attackCooldown = 2f;
    private float attackTimer = 0f;

    private enum State { Idle, Chase, Attack, Patrol, Return }
    private State currentState = State.Idle;

    [Header("Patrol Settings")]
    public float patrolRadius = 15f; // phạm vi patrol rộng
    private float patrolTimer = 0f;
    public float patrolInterval = 3f; // đổi điểm patrol mỗi 3 giây
    private Vector3 initialPosition;
    public float attackRadius = 3f;      // bán kính đánh trúng
    [Header("Loot Settings")]
    public GameObject[] lootPrefabs;       // Các vật phẩm có thể rơi
    [Range(0f, 1f)]
    public float dropChance = 0.9f;        // Xác suất rơi (90%)
    [Header("Extra Loot")]
    public GameObject healthPotionPrefab;


    private Vector3 spawnPoint;
    private Quaternion spawnRotation;

    public LayerMask playerLayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = State.Idle;
        spawnPoint = transform.position;
        spawnRotation = transform.rotation;
        initialPosition = transform.position;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = distanceToPlayer <= detectionRange;

        if (canSeePlayer)
        {
            if (distanceToPlayer <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else
        {
            if (currentState == State.Chase || currentState == State.Attack)
                currentState = State.Return; // mất tầm nhìn thì quay về spawn
            else
                currentState = State.Patrol;
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
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;

            case State.Attack:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                transform.LookAt(player);

                animator.applyRootMotion = false;
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);

                attackTimer += Time.deltaTime;

                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    animator.SetTrigger("DoAttack"); // nếu có anim trigger riêng
                    Invoke(nameof(DealDamage), 0.2f); // sau 0.2s mới check và gây dame
                }
                break;

            case State.Patrol:
                agent.isStopped = false;
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);

                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolInterval || !agent.hasPath || agent.remainingDistance < 0.5f)
                {
                    Vector3 patrolTarget = GetRandomPatrolPoint();
                    agent.SetDestination(patrolTarget);
                    patrolTimer = 0f;
                }
                break;

            case State.Return:
                agent.isStopped = false;
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);

                agent.SetDestination(initialPosition);

                if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
                {
                    currentState = State.Patrol; // quay lại patrol sau khi về vị trí ban đầu
                }
                break;
        }
    }
    void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1.0f,
            attackRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.transform == player)
            {
                Debug.Log("Enemy hit player");
                hit.GetComponent<PlayerHealth>()?.TakeDamage(40f);
                break;
            }
        }
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

    public void TakeDamage(float dmg)
    {
        health -= dmg;
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
        health = 300f; // hoặc dùng maxHealth

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        agent.enabled = true;
        this.enabled = true;

        agent.Warp(spawnPoint);
        transform.rotation = spawnRotation;

        // Reset animator đúng cách
        animator.Rebind();
        animator.Update(0f);

        currentState = State.Idle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition, patrolRadius);
    }
}