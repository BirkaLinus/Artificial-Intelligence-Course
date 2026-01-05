using Lab1;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEditor.AdaptivePerformance.Editor;
using static Lab1.GuardPatrol;

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
    [SerializeField] float fAgentResetSpeed = 15f;

    [Header("CONFUSED STATE SETTINGS")]
    [SerializeField] float fLookAngle = 60f; // how far left/right
    [SerializeField] float fLookTurnSpeed = 120f; // degrees per second

    Quaternion startRotation;
    float fCurrentLookAngle;
    int iLookDirection = 1; // 1 = right, -1 = left
    bool isLookingAround = false;

    [Header("References")]
    PlayerMovement playerMovement;
    Animator animator;
    NavMeshAgent agent;
    FieldOfView FOV;

    public enum STATE { PATROL, CHASE, CAUGHT, CONFUSED }
    public STATE state; //stores the current state.

    private void Awake()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        FOV = GetComponent<FieldOfView>();
    }

    private void Start()
    {
        //Subscribe to event to notify NPC when player gets caught.
        if (playerMovement != null)
        {
            playerMovement.OnCaughtStateChanged.AddListener(OnPlayerCaught);
        }

        //The very first state at start.
        state = STATE.PATROL;
    }
    private void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnCaughtStateChanged.RemoveListener(OnPlayerCaught);
        }
    }

    private void OnPlayerCaught(bool caught)
    {
        isBunnyCaught = caught;
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

        if (isUsingRandomWP)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Length)].transform.position);
            }
            return;
        }

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
        if (isBunnyCaught)
        {
            state = STATE.PATROL;
        }

        if (!FOV.canSeePlayer || isSmokeUp || agent.remainingDistance > fEscapeDistance)
        {
            //state = STATE.CONFUSED;
            EnterConfusedState();
        }

        agent.stoppingDistance = fGoalReachedDistance;
        agent.speed = fAgentChaseSpeed;
        agent.SetDestination(target.position);

        if (agent.remainingDistance <= fGoalReachedDistance && FOV.canSeePlayer && !isSmokeUp)
        {
            EnterCaughtState();
        }
    }

    void EnterCaughtState() //INBETWEEN STATE LOGICS
    {
        state = STATE.CAUGHT; //ACTUALLY ENTER THE STATE
        agent.isStopped = false;
        agent.speed = fAgentResetSpeed;
        agent.stoppingDistance = 0f; //MAKE SURE IT INSTANTLY STAYS WHEN REACHING GOAL
        agent.autoBraking = false;

        iWayPointIndex = GetClosestWayPointIndex();
        agent.SetDestination(wayPoints[iWayPointIndex].position);
    }

    void Caught()
    {
        // Ignoring player completely in this state, just use it to reset to a patrol position again.
        if (!agent.pathPending && agent.remainingDistance <= 0.05f)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();

            agent.autoBraking = true;
            agent.stoppingDistance = fGoalReachedDistance;
            agent.speed = fAgentPatrolSpeed;

            state = STATE.PATROL;
        }
    }

    int GetClosestWayPointIndex()
    {
        if (wayPoints.Length == 0) return -1;

        int closestIndex = 0;
        float closestDistance = Vector3.Distance(transform.position, wayPoints[0].position);

        for (int i = 1; i < wayPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, wayPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    void EnterConfusedState() //ALWAYS CALL THIS BEFORE ENTERING CONFUSED STATE
    {
        if (state == STATE.CONFUSED) return;

        //THE PLACE WE ACTUALLY ENTER CONFUSED STATE FOR REAL.
        state = STATE.CONFUSED;
        agent.isStopped = true;
        startRotation = transform.rotation;
        fCurrentLookAngle = 0f;
        iLookDirection = 1;
        isLookingAround = true;
    }

    void Confused()
    {

        if (FOV.canSeePlayer && agent.remainingDistance >= agent.stoppingDistance)
        {
            agent.isStopped = false;
            state = STATE.CHASE;
            return;
        }

        if (!isLookingAround) //WHEN DONE LOOKING AROUND MOVE TO THE CLOSEST WAYPOINT
        {
            agent.isStopped = false;
            iWayPointIndex = GetClosestWayPointIndex();
            agent.SetDestination(wayPoints[iWayPointIndex].position);
            state = STATE.PATROL;
            return;
        }

        float delta = fLookTurnSpeed * Time.deltaTime * iLookDirection;
        fCurrentLookAngle += delta;
        fCurrentLookAngle = Mathf.Clamp(fCurrentLookAngle, -fLookAngle, fLookAngle);
        transform.rotation = startRotation * Quaternion.Euler(0f, fCurrentLookAngle, 0f);

        if (Mathf.Abs(fCurrentLookAngle) >= fLookAngle)
        {
            iLookDirection *= -1;
            if (iLookDirection == 1)
            {
                isLookingAround = false;
            }
        }

        //DO SOME DELAY STANDING STILL AND THEN BACK TO CLOSEST PATROL
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnterCaughtState();
            CheckPointManager.Instance.RespawnPlayer(other.gameObject);
        }
    }
}
