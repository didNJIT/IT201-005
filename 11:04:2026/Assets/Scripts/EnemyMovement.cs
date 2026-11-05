using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public enum Mode { Chase, Patrol }

    [Header("Behavior")]
    public Mode mode = Mode.Chase;
    public Transform player;
    public float speed = 3.5f;          // set differently for each enemy
    public float chaseRange = 5f;       // Patrol mode: chases if the player gets this close

    [Header("Patrol Only")]
    public Transform[] waypoints;

    private NavMeshAgent navMeshAgent;
    private int current = 0;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || !navMeshAgent.isOnNavMesh) return;

        navMeshAgent.speed = speed;

        float distance = Vector3.Distance(transform.position, player.position);

        // Chase mode always chases; Patrol mode chases only when the player is close
        if (mode == Mode.Chase || distance < chaseRange)
        {
            navMeshAgent.SetDestination(player.position);
            return;
        }

        // Patrol between waypoints
        if (waypoints.Length == 0)
        {
            navMeshAgent.ResetPath();   // no waypoints: stand guard
            return;
        }

        navMeshAgent.SetDestination(waypoints[current].position);

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            current = (current + 1) % waypoints.Length;
        }
    }
}