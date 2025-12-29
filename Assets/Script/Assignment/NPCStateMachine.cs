using Lab1;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCStateMachine : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] Transform[] wayPoints;
    [SerializeField] int iWayPointIndex;

    [Header("Booleans")]
    [SerializeField] bool isUsingRandomWP = false;
    [SerializeField] bool isBunnyCaught = false;
    [SerializeField] bool isSmokeUp = false;

    [Header("Agent Settings")]
    [SerializeField] Transform target;
    [SerializeField] float fGoalReachedDistance = 0.5f;
    [SerializeField] float fEscapeDistance = 20f;
    [SerializeField] float fAgentPatrolSpeed = 3f;
    [SerializeField] float fAgentChaseSpeed = 6f;

    [Header("References")]
    Animator animator;
    NavMeshAgent agent;
    FieldOfView FOV;

    public enum STATE { PATROL, CHASE, CAUGHT, CONFUSED }
    public STATE state; //stores the current state.

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        FOV = GetComponent<FieldOfView>();
    }

    private void Start()
    {
        STATE state = STATE.PATROL; //The very first state at start.
    }

    private void Update()
    {
        switch (state)
        {
            case STATE.PATROL:
                Patrol();
                break;

            case STATE.CHASE:
                Chase();
                break;

            case STATE.CAUGHT:
                Caught();
                break;

            case STATE.CONFUSED:
                Confused();
                break;

        }
    }

    void Patrol()
    {
        //PATROLANIMATION

        //Checks if the player is in sight and switch state accordingly.
        if (FOV.canSeePlayer && !isSmokeUp)
        {
            state = STATE.CHASE;
        }

        agent.stoppingDistance = fGoalReachedDistance;
        agent.speed = fAgentPatrolSpeed;

        //checks if it reaches the waypoint.
        if (agent.remainingDistance < agent.stoppingDistance)
        {
            //increase waypointindex.
            iWayPointIndex++;

            //When reaching the end of the array, move back to the start of the array.
            if (iWayPointIndex >= wayPoints.Length)
            {
                iWayPointIndex = 0;
            }
        }

        //Agent moves to the current waypointindex.
        agent.SetDestination(wayPoints[iWayPointIndex].transform.position); 

    }

    void Chase()
    {
        if (!FOV.canSeePlayer || !isSmokeUp || agent.remainingDistance > fEscapeDistance)
        {
            state = STATE.CONFUSED;
        }

        agent.stoppingDistance = fGoalReachedDistance;
        agent.speed = fAgentChaseSpeed;
        agent.SetDestination(target.position);

        if (agent.remainingDistance <= fGoalReachedDistance)
        {
            state = STATE.CAUGHT;
        }
    }

    void Caught()
    {
        agent.speed = 0f;
    }

    void Confused()
    {
        //DO SOME DELAY STANDING STILL AND THEN BACK TO CLOSEST PATROL
    }

}
