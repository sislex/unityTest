using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loopRoute = true;
    [SerializeField] private bool randomOrder = false;

    [Header("Movement")]
    [SerializeField] private float waitTimeAtPoint = 1.2f;
    [SerializeField] private float arriveDistance = 0.3f;

    public void SetWaypoints(Transform[] points)
    {
        waypoints = points;
    }

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        currentWaypointIndex = 0;
        waitTimer = 0f;

        if (waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint(currentWaypointIndex);
        }
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        if (agent.remainingDistance > Mathf.Max(arriveDistance, agent.stoppingDistance))
        {
            return;
        }

        waitTimer += Time.deltaTime;
        if (waitTimer < waitTimeAtPoint)
        {
            return;
        }

        waitTimer = 0f;
        int nextIndex = GetNextWaypointIndex();

        if (nextIndex < 0)
        {
            enabled = false;
            return;
        }

        currentWaypointIndex = nextIndex;
        MoveToWaypoint(currentWaypointIndex);
    }

    private int GetNextWaypointIndex()
    {
        if (randomOrder)
        {
            if (waypoints.Length == 1)
            {
                return 0;
            }

            int next = currentWaypointIndex;
            while (next == currentWaypointIndex)
            {
                next = Random.Range(0, waypoints.Length);
            }
            return next;
        }

        int sequentialNext = currentWaypointIndex + 1;
        if (sequentialNext < waypoints.Length)
        {
            return sequentialNext;
        }

        if (loopRoute)
        {
            return 0;
        }

        return -1;
    }

    private void MoveToWaypoint(int index)
    {
        Transform target = waypoints[index];
        if (target == null)
        {
            return;
        }

        agent.SetDestination(target.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 1f);
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                continue;
            }

            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        if (loopRoute && waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
#endif
}


