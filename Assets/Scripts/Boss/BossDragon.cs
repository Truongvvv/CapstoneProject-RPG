using UnityEngine;
using UnityEngine.AI;

public class BossDragon : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 25f;
    public float attackRange = 3f;
    public float retreatRange = 6f;
    public float health = 500f;
    public float retreatThreshold = 100f; // Nếu máu < retreatThreshold thì rút

    [Header("Attack Settings")]
    public float attackRadius = 2f;
    public LayerMask playerLayer;
    private bool hasDealtDamage = false;
    public float attackCooldown = 1.5f;
    private float attackTimer = 0f;
    public int expReward = 200; // EXP khi chết

    [Header("Roar Settings")]
    public float roarDuration = 3f;
    private float roarTimer = 0f;
    private bool hasRoared = false;
    private bool isRoaring = false;

    [Header("VFX Settings")]
    public GameObject chargeVFXPrefab;   // prefab hiệu ứng gồng
    public Transform vfxSpawnPoint;      // điểm spawn VFX (có thể là empty object gắn vào boss)
    private GameObject activeChargeVFX;  // hiệu ứng đang chạy

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 originalPosition;

    private enum State { Idle, Fly, Roar, Chase, Attack, Retreat, Return, SpecialSkill }
    private State currentState;

    [Header("Fly Settings")]
    public float flyDuration = 5f;   // thời gian bay
    private float flyTimer = 0f;
    private bool hasFlown = false;

    [Header("Special Skill Settings")]
    public float specialSkillCooldown = 10f;
    public float specialSkillDuration = 3f; // thời gian thi triển skill
    public float specialSkillRadius = 5f;
    public float specialSkillDamage = 150f;
    public GameObject specialSkillVFX;

    private float combatTimer = 0f;
    private bool isUsingSpecialSkill = false;
    private float specialSkillTimer = 0f;


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

        // FSM điều kiện
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
            if (!hasFlown)
            {
                currentState = State.Fly;
            }
            else if (!hasRoared || isRoaring) // CHỈ ROAR KHI CHƯA ROAR HOẶC ĐANG ROAR
            {
                currentState = State.Roar;
            }
            else if (distance <= attackRange)
            {
                currentState = State.Attack;
            }
            else
            {
                currentState = State.Chase;
            }

            // Chỉ tính combatTimer khi boss đang Chase hoặc Attack
            if (currentState == State.Chase || currentState == State.Attack)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer >= specialSkillCooldown && !isUsingSpecialSkill)
                {
                    currentState = State.SpecialSkill;
                }
            }
        }
        else
        {
            // Reset khi player chạy khỏi detectionRange
            combatTimer = 0f;
            hasRoared = false;
            roarTimer = 0f;

            float backDistance = Vector3.Distance(transform.position, originalPosition);
            if (backDistance > 0.5f)
                currentState = State.Return;
            else
                currentState = State.Idle;
        }

        // Reset damage khi không ở trạng thái Attack
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

            case State.Fly:
                if (agent.enabled) agent.enabled = false; // tắt hẳn navmesh
                transform.LookAt(player);

                if (!hasFlown && flyTimer == 0f)
                {
                    animator.SetTrigger("FlyUp");
                    // nếu cần thì tắt collider gốc luôn
                    Collider col = GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }

                flyTimer += Time.deltaTime;

                if (flyTimer >= flyDuration)
                {
                    hasFlown = true;
                    flyTimer = 0f;

                    // bật lại navmesh + collider
                    agent.enabled = true;
                    Collider col = GetComponent<Collider>();
                    if (col != null) col.enabled = true;

                    currentState = State.Roar;
                }
                break;

            case State.Roar:
                isRoaring = true;

                if (agent.enabled)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }

                Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.LookAt(lookPos);

                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", false);

                if (!hasRoared && activeChargeVFX == null)
                {
                    animator.SetTrigger("Roar");
                    hasRoared = true;

                    if (chargeVFXPrefab != null && vfxSpawnPoint != null)
                    {
                        activeChargeVFX = Instantiate(chargeVFXPrefab, vfxSpawnPoint.position, Quaternion.identity, vfxSpawnPoint);
                    }
                }

                roarTimer += Time.deltaTime;
                if (roarTimer >= roarDuration)
                {
                    isRoaring = false;   // báo hiệu roar đã xong
                    currentState = State.Chase;
                    roarTimer = 0f;

                    if (agent.enabled)
                    {
                        agent.isStopped = false;
                    }

                    if (activeChargeVFX != null)
                    {
                        Destroy(activeChargeVFX);
                        activeChargeVFX = null;
                    }
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
                agent.velocity = Vector3.zero;
                transform.LookAt(player);
                animator.applyRootMotion = false;
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);

                attackTimer += Time.deltaTime;
                
                break;

            case State.Retreat:
                agent.isStopped = false;
                Vector3 retreatDir = (transform.position - player.position).normalized;
                Vector3 retreatTarget = transform.position + retreatDir * 6f;
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

            case State.SpecialSkill:
                if (!isUsingSpecialSkill)
                {
                    isUsingSpecialSkill = true;
                    specialSkillTimer = 0f;

                    // Boss đứng yên, bỏ qua player
                    if (agent.enabled)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                        agent.velocity = Vector3.zero;
                    }

                    // Play anim đặc biệt
                    animator.SetTrigger("SpecialSkill");

                    // Spawn VFX
                    if (specialSkillVFX != null)
                    {
                        GameObject vfx = Instantiate(specialSkillVFX, transform.position, Quaternion.identity, transform);
                        Destroy(vfx, specialSkillDuration); // sống đúng bằng thời gian thi triển skill
                    }

                    // Damage AOE
                    Collider[] hits = Physics.OverlapSphere(transform.position, specialSkillRadius, playerLayer);
                    foreach (Collider hit in hits)
                    {
                        hit.GetComponent<PlayerHealth>()?.TakeDamage(specialSkillDamage);
                    }
                }

                // Chỉ đếm thời gian skill, không quan tâm player
                specialSkillTimer += Time.deltaTime;
                if (specialSkillTimer >= specialSkillDuration)
                {
                    isUsingSpecialSkill = false;
                    combatTimer = 0f;

                    // Sau khi xong skill mới quay về FSM bình thường
                    float dist = Vector3.Distance(transform.position, player.position);
                    if (dist <= attackRange)
                        currentState = State.Attack;
                    else if (dist <= detectionRange)
                        currentState = State.Chase;
                    else
                        currentState = State.Return;
                }
                break;

        }
    }
   
    public void DealDamage()
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
                hit.GetComponent<PlayerHealth>()?.TakeDamage(100f);
                break; // chỉ gây damage 1 lần
            }
        }
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
        Destroy(gameObject, 8f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, specialSkillRadius);
    }
}