using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Guard : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent; // Reference to the NavMeshAgent component
    [SerializeField] private Transform target; // The player transform
    [SerializeField] private Transform sightSensor; // Child object acting as the sight sensor
    [SerializeField] private Transform soundSensor; // Child object acting as the sound sensor
    [SerializeField] private float sightRange = 10f; // Maximum distance to see the player
    [SerializeField] private float fieldOfViewAngle = 110f; // Angle for field of view
    [SerializeField] private List<Transform> patrolPoints; // List of patrol points
    [SerializeField] private float waitAtPatrolPointDuration = 2f; // Duration to wait at each patrol point
    [SerializeField] private LayerMask obstructionLayer; // Layer for walls or obstacles
    [SerializeField] private float investigationTime = 5f; // Time to investigate the sound
    [SerializeField] private float soundDetectionRadius = 3f; // Radius for sound detection

    private Vector3 lastKnownPlayerPosition; // Last position where the player was seen or heard
    private int currentPatrolIndex; // Current index for patrol points
    private enum GuardState { Patrolling, Investigating, Pursuing }
    private GuardState currentState;

    void Start()
    {
        currentState = GuardState.Patrolling;
        currentPatrolIndex = 0;
        StartPatrolling();
    }

    void Update()
    {
        switch (currentState)
        {
            case GuardState.Patrolling:
                Patrol();
                break;
            case GuardState.Investigating:
                Investigate();
                break;
            case GuardState.Pursuing:
                Pursue();
                break;
        }
    }

    private void StartPatrolling()
    {
        if (patrolPoints.Count > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            Debug.Log("Guard is patrolling to point: " + currentPatrolIndex);
        }
    }

    private void Patrol()
    {
        if (agent.remainingDistance < 0.5f) // Check if the guard reached the patrol point
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count; // Cycle to the next point
            StartCoroutine(WaitAtPatrolPoint());
        }

        // Check for sight
        if (CanSeePlayer())
        {
            currentState = GuardState.Pursuing;
            Debug.Log("Player detected visually! Switching to Pursuing.");
        }

        // Check for sound detection within the sound detection radius
        if (Vector3.Distance(transform.position, target.position) <= soundDetectionRadius &&
            target.GetComponent<PlayerController>().IsMakingNoise())
        {
            lastKnownPlayerPosition = target.position; // Last known position of sound
            currentState = GuardState.Investigating;
            Debug.Log("Heard something! Investigating...");
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        yield return new WaitForSeconds(waitAtPatrolPointDuration);
        StartPatrolling(); // Start moving to the next patrol point
        Debug.Log("Moving to next patrol point.");
    }

    private void Investigate()
    {
        // Turn to face the last known sound direction
        Vector3 directionToSound = lastKnownPlayerPosition - transform.position;
        directionToSound.y = 0; // Ignore vertical difference
        if (directionToSound.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToSound);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Move towards the last known player position
        agent.SetDestination(lastKnownPlayerPosition);
        Debug.Log("Investigating sound at position: " + lastKnownPlayerPosition);

        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
        {
            // Wait for a brief moment before returning to patrol
            StartCoroutine(WaitAndReturnToPatrol());
        }

        // Check for sight again
        if (CanSeePlayer())
        {
            currentState = GuardState.Pursuing;
            Debug.Log("Player spotted while investigating! Pursuing.");
        }
    }

    private void Pursue()
    {
        // Continuously move towards the player
        agent.SetDestination(target.position);
        Debug.Log("Chasing the player!");

        // Check if the player is still in sight
        if (!CanSeePlayer())
        {
            lastKnownPlayerPosition = target.position; // Remember the last known position
            currentState = GuardState.Investigating;
            Debug.Log("Lost sight of the player! Investigating.");
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

        // Check if player is within the field of view angle
        if (dotProduct > Mathf.Cos(fieldOfViewAngle * 0.5f * Mathf.Deg2Rad))
        {
            // Check if the player is within sight range
            if (Vector3.Distance(transform.position, target.position) <= sightRange)
            {
                // Raycast to check for obstacles
                RaycastHit hit;
                if (Physics.Raycast(sightSensor.position, directionToPlayer, out hit, sightRange, obstructionLayer))
                {
                    // Check if the ray hit the player
                    if (hit.transform == target)
                    {
                        return true; // Player is in sight
                    }
                }
            }
        }
        return false; // Player is not visible
    }

    private IEnumerator WaitAndReturnToPatrol()
    {
        yield return new WaitForSeconds(investigationTime); // Wait for investigation time
        currentState = GuardState.Patrolling; // Return to patrolling
        Debug.Log("Returning to patrol...");
    }

    private void OnDrawGizmos()
    {
        // Visualize the sight range and field of view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.DrawLine(sightSensor.position, sightSensor.position + (transform.forward * sightRange));

        // Visualize sound detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundDetectionRadius);
    }
}
