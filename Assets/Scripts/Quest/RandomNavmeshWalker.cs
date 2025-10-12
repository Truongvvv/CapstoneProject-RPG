using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomNavmeshWalker : MonoBehaviour
{
    [Header("Thiết lập đi bộ")]
    public float radius = 100f;          // bán kính di chuyển
    public float moveDuration = 30f;     // đi bao lâu thì nghỉ
    public float restTime = 5f;          // nghỉ bao lâu
    public float pointPickInterval = 5f; // khoảng thời gian tối đa chọn điểm mới

    [Header("Cảm biến chướng ngại vật")]
    public float senseRadius = 5f;       // phạm vi cảm nhận vật cản
    public LayerMask obstacleMask;       // layer các vật cản (trừ đường, player)

    [Header("Nói chuyện với Player")]
    public float lookAtSpeed = 5f;      // tốc độ quay mặt
    private Animator anim;

    private NavMeshAgent agent;
    private Vector3 startPos;

    private float stateTimer;
    private float pointTimer;
    private bool isResting;
    private bool isTalkingWithPlayer;
    private Transform playerTarget;     // lưu Player để quay mặt


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPos = transform.position;

        stateTimer = 0f;
        pointTimer = 0f;
        isResting = false;
        isTalkingWithPlayer = false;
        anim = GetComponent<Animator>();

        PickRandomDestination();
    }

    void Update()
    {
        // ----- Khi nói chuyện với Player -----
        if (isTalkingWithPlayer && playerTarget != null)
        {
            if (agent.hasPath) agent.ResetPath();

            // Quay mặt về phía Player
            Vector3 dir = (playerTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookAtSpeed);
            }

            // Bật animation Talk
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isTalking", true);
            }

            return;
        }

        // ----- Khi không nói chuyện -----
        stateTimer += Time.deltaTime;

        if (isResting)
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isTalking", false);
            }

            if (stateTimer >= restTime)
            {
                isResting = false;
                stateTimer = 0f;
                PickRandomDestination();
            }
        }
        else
        {
            if (anim != null)
            {
                bool moving = agent.velocity.magnitude > 0.1f;
                anim.SetBool("isWalking", moving);
                anim.SetBool("isTalking", false);
            }

            pointTimer += Time.deltaTime;

            if (stateTimer >= moveDuration)
            {
                isResting = true;
                stateTimer = 0f;
                if (agent.hasPath) agent.ResetPath();
                return;
            }

            if ((!agent.pathPending && agent.remainingDistance < 1f) || pointTimer >= pointPickInterval)
            {
                PickRandomDestination();
                pointTimer = 0f;
            }
        }
    }

    void PickRandomDestination()
    {
        Collider[] obstacles = Physics.OverlapSphere(transform.position, senseRadius, obstacleMask);

        if (obstacles.Length > 0)
        {
            Collider nearest = obstacles[0];
            float minDist = Vector3.Distance(transform.position, nearest.transform.position);

            foreach (var ob in obstacles)
            {
                float dist = Vector3.Distance(transform.position, ob.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = ob;
                }
            }

            Vector3 awayDir = (transform.position - nearest.transform.position).normalized;
            Vector3 avoidPoint = transform.position + awayDir * senseRadius * 1.5f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(avoidPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 rand = Random.insideUnitCircle * radius;
            Vector3 target = startPos + new Vector3(rand.x, 0, rand.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        agent.SetDestination(transform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? startPos : transform.position;
        Gizmos.DrawWireSphere(center, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, senseRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTalkingWithPlayer = true;
            playerTarget = other.transform;
            if (agent.hasPath) agent.ResetPath();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTalkingWithPlayer = false;
            playerTarget = null;
            stateTimer = 0f;
            pointTimer = 0f;

            if (!isResting)
                PickRandomDestination();
        }
    }
}