using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float retreatRange = 5f;
    public float health = 100f;
    public float retreatThreshold = 30f; // Nếu máu thấp hơn thì chạy trốn

    public float attackRadius = 1.5f;      // bán kính đánh trúng
    public LayerMask playerLayer;          // layer của player
    private bool hasDealtDamage = false;   // tránh gây damage nhiều lần
    public float attackCooldown = 1.0f;
    private float attackTimer = 0f;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 originalPosition;
    private enum State { Idle, Roar, Chase, Attack, Retreat, Return }
    private State currentState;

    public float roarDuration = 1.5f;
    private float roarTimer = 0f;
    private bool hasRoared = false;

    [Header("Loot Settings")]
    public GameObject[] lootPrefabs;       // Các vật phẩm có thể rơi
    [Range(0f, 1f)]
    public float dropChance = 0.9f;        // Xác suất rơi (30%)


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = State.Idle;
        originalPosition = transform.position;

    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (health <= retreatThreshold && distance < retreatRange)
        {
            currentState = State.Retreat;
        }
        else if (distance <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance <= detectionRange)
        {
            if (!hasRoared)
            {
                currentState = State.Roar;
            }
            else
            {
                currentState = State.Chase;
            }
        }
        else
        {
            // Mất dấu player → reset roar
            hasRoared = false;
            roarTimer = 0f;

            float backDistance = Vector3.Distance(transform.position, originalPosition);
            if (backDistance > 0.5f)
                currentState = State.Return;
            else
                currentState = State.Idle;
        }

        if (distance > detectionRange)
        {
            hasRoared = false;
            roarTimer = 0f;
        }

        if (currentState != State.Attack)
        {
            hasDealtDamage = false;
            attackTimer = 0f;
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
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;

            case State.Roar:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                transform.LookAt(player);
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", false);
                animator.SetTrigger("Roar");

                roarTimer += Time.deltaTime;
                if (roarTimer >= roarDuration)
                {
                    hasRoared = true;
                    currentState = State.Chase;
                    roarTimer = 0f;
                }
                break;

            case State.Attack:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                transform.LookAt(player);
                animator.applyRootMotion = false;
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);

                attackTimer += Time.deltaTime;

                if (!hasDealtDamage && attackTimer >= attackCooldown)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, attackRadius, playerLayer);
                    foreach (Collider hit in hits)
                    {
                        if (hit.transform == player)
                        {
                            Debug.Log("Enemy hit player");
                            hit.GetComponent<PlayerHealth>()?.TakeDamage(20f);
                            hasDealtDamage = true;
                            attackTimer = 0f;
                            break;
                        }
                    }
                }
                break;

            case State.Retreat:
                agent.isStopped = false;
                Vector3 retreatDir = (transform.position - player.position).normalized;
                Vector3 retreatTarget = transform.position + retreatDir * 5f;
                agent.SetDestination(retreatTarget);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;

            case State.Return:
                agent.isStopped = false;
                agent.SetDestination(originalPosition);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;
        }
    }

    //Hàm rớt vật phẩm
    void TryDropLoot()
    {
        if (lootPrefabs.Length == 0) return;

        if (Random.value <= dropChance)
        {
            int index = Random.Range(0, lootPrefabs.Length);
            Instantiate(lootPrefabs[index], transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Weapon")) // Gậy gắn tag "Weapon"
        //{
        //    Debug.Log("Gay 20dame ");
        //    TakeDamage(20); // Hoặc dùng gậy chứa script chỉ định damage
        //}

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

    // Hàm để nhận sát thương
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
            
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");
        agent.isStopped = true;
        this.enabled = false;
        TryDropLoot(); // ← Gọi hàm rớt vật phẩm
        // Xóa enemy sau 2 giây
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
