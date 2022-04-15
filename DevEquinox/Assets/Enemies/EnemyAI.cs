using UnityEngine;
using UnityEngine.AI;
using System.Collections;


[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum WanderType {Random, Waypoint};

    WaitForSeconds updateStateRate = new WaitForSeconds(0.2f);

    [SerializeField] private Transform target;
    public WanderType wanderType = WanderType.Random;
    public float wanderSpeed = 1.5f, chaseSpeed = 3f;
    public Transform[] waypoints;
    public float fov = 120f;
    public float viewDistance = 10f;
    public float wanderRadius = 10f;
    public float losePlayerSightThreshold = 50f;
    public bool isAware = false;

    [SerializeField] private LayerMask attackableLayer;
    private bool isDetecting = false;
    private Vector3 wanderPoint;
    private NavMeshAgent agent;
    private int waypointIndex = 0;
    private Animator animator;
    private float loseTimer = 0;
    private HealthSystem healthSystem;
    public GameObject ragdollVersion;
    private Transform selfTransform;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        selfTransform = transform;
    }

    void OnEnable()
    {
        target = null;
        wanderPoint = RandomWanderPoint();
        StartCoroutine(StateUpdate());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator StateUpdate()
    {
        if (healthSystem.IsDead) {
            StopAllCoroutines();
            yield return null;
        }

        if (isAware && target != null) {
            agent.SetDestination(target.position);
            agent.speed = chaseSpeed;
            animator.speed = chaseSpeed;
            if (!isDetecting) {
                loseTimer += Time.deltaTime;
                if (loseTimer >= losePlayerSightThreshold) {
                    ResetAttention();
                    Debug.Log("Lost prey sight");
                }
            }
        }
        else {
            if (isAware && target == null) ResetAttention();
            Wander();
            agent.speed = wanderSpeed;
            animator.speed = wanderSpeed;
        }
        yield return null;
        SearchForPlayer();
        yield return updateStateRate;
        StartCoroutine(StateUpdate());
    }

    public void SearchForPlayer()
    {
        Transform result = NearestToChase();
        if ((isDetecting || !isAware) && result != null) target = result;
        if (result == null) {
            isDetecting = false;
            return;
        }

        if (Vector3.Angle(Vector3.forward, selfTransform.InverseTransformPoint(target.position)) < fov / 2f) {
            Debug.DrawLine(selfTransform.position, target.position);
            if (Physics.Linecast(selfTransform.position, target.position, out RaycastHit hit, -1))
            {
                Debug.DrawLine(selfTransform.position, hit.point);
                if (hit.collider.CompareTag(GlobalAccess._Player))
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
            if (Vector3.Distance(selfTransform.position, wanderPoint) < 2f)
            {
                wanderPoint = RandomWanderPoint();
            }
            else
            {
                agent.SetDestination(wanderPoint);
            }
        }
        else
        {
            if (waypoints.Length > 1)
            {
                if (Vector3.Distance(waypoints[waypointIndex].position, selfTransform.position) < 2f)
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

    public void Die(Vector3 point, Vector3 impactForce)
    {
        Instantiate(ragdollVersion, selfTransform.position, selfTransform.rotation).GetComponent<Ragdoller>().ApplyForce(point, impactForce);
        gameObject.SetActive(false);
    }

    public Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + selfTransform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomPoint, out navHit, wanderRadius, -1);
        return new Vector3(navHit.position.x, selfTransform.position.y, navHit.position.z);
    }

    private Transform NearestToChase()
    {
        Transform nearestTransform = null;
        float nearestDistance = viewDistance;
        Collider[] attackables = Physics.OverlapSphere(selfTransform.position, viewDistance, attackableLayer);
        for(uint i = 0; i < attackables.Length; i++) {
            float distance = Vector3.Distance(selfTransform.position, attackables[i].gameObject.transform.position);
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearestTransform = attackables[i].gameObject.transform;
            }
        }
        return nearestTransform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fov / 2f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fov / 2f, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * selfTransform.forward;
        Vector3 rightRayDirection = rightRayRotation * selfTransform.forward;
        Gizmos.DrawRay(selfTransform.position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(selfTransform.position, rightRayDirection * viewDistance);
    }
}
