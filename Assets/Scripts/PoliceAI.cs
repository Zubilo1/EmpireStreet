
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PoliceAI : MonoBehaviour
{
    public enum PoliceState { Patrolling, Chasing, Searching }
    public PoliceState currentState = PoliceState.Patrolling;

    [Header("Ссылки")]
    public Transform player;

    public Transform target { get => player; set => player = value; }
    public Transform Target { get => player; set => player = value; }

    [Header("Обзор")]
    public float visionRange = 18f;
    [Range(30f, 180f)] public float fieldOfView = 120f;
    public LayerMask obstacleMask = ~0;

    [Header("Движение")]
    public float moveSpeed = 3.5f;
    public float chaseMultiplier = 1.3f;
    public float searchDuration = 20f;
    public float searchRadius = 8f;

    NavMeshAgent agent;
    Vector3 lastKnownPosition;
    Vector3 patrolDestination;
    float lastSeenTime;
    bool hasLastKnownPosition;
    bool hasPatrolDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1.5f;

        FindPlayer();
    }

    void FindPlayer()
    {
        if (player != null) return;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found == null) found = GameObject.Find("Player");

        if (found != null) player = found.transform;
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(
                transform.position, out NavMeshHit hit,
                5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            return;
        }

        if (player == null) FindPlayer();
        if (player == null) return;

        int wanted = GameManager.I != null
            ? GameManager.I.WantedLevel : 0;

        if (wanted <= 0)
        {
            hasLastKnownPosition = false;
            PatrolBehavior();
            return;
        }

        if (CheckLineOfSight())
        {
            lastKnownPosition = player.position;
            lastSeenTime = Time.time;
            hasLastKnownPosition = true;

            currentState = PoliceState.Chasing;
            agent.speed = moveSpeed * chaseMultiplier;
            agent.isStopped = false;

            SetDestinationSafely(player.position);
        }
        else if (hasLastKnownPosition)
        {
            currentState = PoliceState.Searching;
            agent.speed = moveSpeed;
            agent.isStopped = false;

            if (!agent.pathPending &&
                (!agent.hasPath || agent.remainingDistance < 1.5f))
            {
                Vector3 searchPoint = CitizenAI.RandomNavSphere(
                    lastKnownPosition, searchRadius, NavMesh.AllAreas);

                SetDestinationSafely(searchPoint);
            }

            if (Time.time - lastSeenTime >= searchDuration)
            {
                hasLastKnownPosition = false;
                hasPatrolDestination = false;
                agent.ResetPath();

                if (GameManager.I != null)
                    GameManager.I.ClearWanted();

                currentState = PoliceState.Patrolling;
            }
        }
        else
        {
            PatrolBehavior();
        }
    }

    bool CheckLineOfSight()
    {
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 targetPoint = player.position + Vector3.up;
        Vector3 direction = targetPoint - eye;
        float distance = direction.magnitude;

        if (distance > visionRange || distance < 0.01f)
            return false;

        if (Vector3.Angle(transform.forward, direction) >
            fieldOfView * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(
            eye, direction.normalized, distance,
            obstacleMask, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // Не считаем коллайдеры самого полицейского препятствием.
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
                continue;

            // Попадание в игрока означает, что обзор свободен.
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
                return true;

            // Любой другой объект в маске блокирует обзор.
            return false;
        }

        return true;
    }

    void SetDestinationSafely(Vector3 destination)
    {
        if (NavMesh.SamplePosition(
            destination, out NavMeshHit hit,
            5f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();

            if (agent.CalculatePath(hit.position, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    void PatrolBehavior()
    {
        currentState = PoliceState.Patrolling;
        agent.speed = moveSpeed;
        agent.isStopped = false;

        if (!hasPatrolDestination ||
            (!agent.pathPending &&
             (!agent.hasPath || agent.remainingDistance < 1.5f)))
        {
            Vector3 point = CitizenAI.RandomNavSphere(
                transform.position, 15f, NavMesh.AllAreas);

            if ((point - transform.position).sqrMagnitude > 1f)
            {
                SetDestinationSafely(point);
                patrolDestination = point;
                hasPatrolDestination = true;
            }
        }
    }
}
