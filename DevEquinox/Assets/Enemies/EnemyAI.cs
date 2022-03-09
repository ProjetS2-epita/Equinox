using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum WanderType {Random, Waypoint};

    [SerializeField] private Transform target;
    public WanderType wanderType = WanderType.Random;
    public float wanderSpeed = 1.5f, chaseSpeed = 3f;
    public Transform[] waypoints;
    public float fov = 120f;
    public float viewDistance = 10f;
    public float wanderRadius = 7f;
    public float losePlayerSightThreshold = 50f;
    public bool isAware = false;

    [SerializeField] private LayerMask attackableLayer;
    private bool isDetecting = false;
    private Vector3 wanderPoint;
    private NavMeshAgent agent;
    private int waypointIndex = 0;
    private int debugWanderMaxFrame = 200, debugWanderFrame = 0;
    private Animator animator;
    private float loseTimer = 0;
    private HealthSystem healthSystem;
    private bool IsDead = false;

    void Start()
    {
        target = null;
        healthSystem = GetComponent<HealthSystem>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        wanderPoint = RandomWanderPoint();
    }

    void Update()
    {
        if (healthSystem.IsDead) {
            Debug.Log("zombie dead");
            if (!IsDead) {
                IsDead = true;
                Die();
            }
            return;
        }

        if (isAware && target != null)
        {
            agent.SetDestination(target.position);
            agent.speed = chaseSpeed;
            animator.speed = chaseSpeed;
            if (!isDetecting)
            {
                loseTimer += Time.deltaTime;
                if (loseTimer >= losePlayerSightThreshold)
                {
                    ResetAttention();
                    Debug.Log("Lost prey sight");
                }
            }
        }
        else
        {
            ResetAttention();
            Wander();
            agent.speed = wanderSpeed;
            animator.speed = wanderSpeed;
        }
        SearchForPlayer();
    }

    public void SearchForPlayer()
    {
        Transform result = NearestToChase();
        if ((isDetecting || !isAware) && result != null) target = result;
        if (result == null) {
            isDetecting = false;
            return;
        }

        if (Vector3.Angle(Vector3.forward, transform.InverseTransformPoint(target.position)) < fov / 2f) {
            Debug.DrawLine(transform.position, target.position);
            if (Physics.Linecast(transform.position, target.position, out RaycastHit hit, -1))
            {
                Debug.DrawLine(transform.position, hit.point);
                if (hit.collider.CompareTag("Player"))
                {
                    OnAware(hit.collider.gameObject.transform);
                    return;
                }
            }
        }
        isDetecting = false;
    }

    public void OnAware(Transform target)
    {
        this.target = target;
        isAware = true;
        isDetecting = true;
        loseTimer = 0;
        Debug.Log($"aware of : {target.name}");
    }

    private void ResetAttention()
    {
        target = null;
        isAware = false;
        loseTimer = 0;
    }

    public void Wander()
    {
        if (wanderType == WanderType.Random)
        {
            if (Vector3.Distance(transform.position, wanderPoint) < 2f || debugWanderFrame >= debugWanderMaxFrame)
            {
                debugWanderFrame = 0;
                wanderPoint = RandomWanderPoint();
            }
            else
            {
                debugWanderFrame++;
                agent.SetDestination(wanderPoint);
            }
        }
        else
        {
            if (waypoints.Length > 1)
            {
                if (Vector3.Distance(waypoints[waypointIndex].position, transform.position) < 2f)
                {
                    if (waypointIndex == waypoints.Length - 1) waypointIndex = 0;
                    else waypointIndex++;
                }
                else
                {
                    agent.SetDestination(waypoints[waypointIndex].position);
                }
            }
            else Debug.LogWarning("More than 1 waypoint needed for IA: " + gameObject.name);
        }
    }

    private void Die()
    {
        agent.speed = 0;
        animator.enabled = false;
        //GetComponent<Collider>().enabled = false;
        Destroy(gameObject);
    }

    public Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomPoint, out navHit, wanderRadius, -1);
        return new Vector3(navHit.position.x, transform.position.y, navHit.position.z);
    }

    private Transform NearestToChase()
    {
        Transform nearestTransform = null;
        float nearestDistance = viewDistance;
        Collider[] attackables = Physics.OverlapSphere(transform.position, viewDistance, attackableLayer);
        foreach (Collider attackable in attackables) {
            float distance = Vector3.Distance(transform.position, attackable.gameObject.transform.position);
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearestTransform = attackable.gameObject.transform;
            }
        }
        return nearestTransform;
    }

    private void OnDrawGizmosSelected()
    {
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        */

        Gizmos.color = Color.magenta;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fov / 2f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fov / 2f, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(transform.position, rightRayDirection * viewDistance);
    }
}
