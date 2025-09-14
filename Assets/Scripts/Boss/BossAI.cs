using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 20f;
    public float attackRange = 3f;
    public float retreatRange = 5f;
    public float retreatThreshold = 50f;
    public float health = 300f;

    public float roarDuration = 2f; // thời gian gồng
    private float roarTimer = 0f;
    private bool hasRoared = false;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 originalPosition;

    private enum State { Idle, Gong, Chase, Attack, Retreat, Return }
    private State currentState;

    public float attackRadius = 2f;
    public LayerMask playerLayer;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private bool hasDealtDamage = false;

    public Transform attackPoint;
    private bool hasStartedGongAnim = false;
    public int expReward = 200; // EXP khi chết

    private Renderer[] renderers;
    private Color[] originalColors;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originalPosition = transform.position;
        currentState = State.Idle;

        // Lấy tất cả renderer trong boss (bao gồm SkinnedMeshRenderer)
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // State logic
        if (health <= retreatThreshold && distance < retreatRange)
        {
            currentState = State.Retreat;
        }
        else if (distance <= attackRange && hasRoared)
        {
            currentState = State.Attack;
        }
        else if (distance <= detectionRange)
        {
            if (!hasRoared)
            {
                currentState = State.Gong;
            }
            else
            {
                currentState = State.Chase;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, originalPosition) > 0.5f)
                currentState = State.Return;
            else
                currentState = State.Idle;
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
                animator.SetBool("isAttacking", false);
                break;

            case State.Gong:
                agent.isStopped = true;
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", false);
                transform.LookAt(player);

                if (!hasStartedGongAnim)
                {
                    animator.SetTrigger("Gong");           //  chỉ gọi 1 lần
                    hasStartedGongAnim = true;
                }

                roarTimer += Time.deltaTime;
                if (roarTimer >= roarDuration)
                {
                    hasRoared = true;
                    roarTimer = 0f;
                    hasStartedGongAnim = false;             // reset lại để reuse sau này
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);
                break;

            case State.Attack:
                agent.isStopped = true;
                transform.LookAt(player);
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);

                attackTimer += Time.deltaTime;
                //if (!hasDealtDamage && attackTimer >= attackCooldown)
                
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
    public void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Boss hit player!");
                hit.GetComponent<PlayerHealth>()?.TakeDamage(100f);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (renderers != null && renderers.Length > 0)
        {
            StartCoroutine(FlashHitColor());
        }

        if (health <= 0)
        {
            Die();
        }
    }
    private IEnumerator FlashHitColor()
    {
        foreach (var r in renderers)
        {
            r.material.color = hitColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    void Die()
    {
        if (PlayerQuestManager.Instance != null)
        {
            PlayerQuestManager.Instance.AddKill(gameObject.tag);
        }

        // Vô hiệu hóa collider để player không còn va chạm với enemy đã chết
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        agent.isStopped = true;

        // Tắt các trạng thái khác để không bị override anim Die
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Die");

        this.enabled = false;

        // Gọi EXP cho player
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.GainExp(expReward);
        }
        Destroy(gameObject, 10f);
    }

    private void EnablePhysics(bool enable)
    {
        if (agent != null) agent.enabled = enable;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = enable;
        }

        // Nếu dùng Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !enable;
            rb.useGravity = enable;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}