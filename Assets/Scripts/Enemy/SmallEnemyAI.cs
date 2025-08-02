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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
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

    void Die()
    {
        animator.SetTrigger("Die");
        agent.isStopped = true;
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}