using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class RedLightStateMachine : MonoBehaviour
{

    [Header("References")]
    NavMeshAgent agent;

    [SerializeField] GameObject goPlayer;
    [SerializeField] Transform tStartPos;
    [SerializeField] Transform tGoalPos;

    [SerializeField] Vector3 vIdleStartPos;
    [SerializeField] Vector3 vRedStartPos; //Store the value to see if the player will survive the redState.
    [SerializeField] Vector3 playerPostion;
    [SerializeField] Vector3 agentPostion;

    [Header("Agent Stats")]
    [SerializeField] float fIdleResetSpeed;
    [SerializeField] float fGreenSpeed;
    [SerializeField] float fYellowSpeed;
    [SerializeField] float fRedSpeed;

    [Header("Agent Booleans")]
    [SerializeField] bool hasEnteredRedState;
    [SerializeField] bool redDelayRunning;
    [SerializeField] bool idleDelayRunning;

    public enum STATE { IDLE, GREEN, YELLOW, RED, EXECUTE }
    public STATE state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        vIdleStartPos = transform.position;
        state = STATE.IDLE;
    }

    private void Update()
    {

        switch (state)
        {
            case STATE.IDLE:
                Idle();
                break;
            case STATE.GREEN:
                Green();
                break;
            case STATE.YELLOW:
                Yellow();
                break;
            case STATE.RED:
                Red();
                break;
                case STATE.EXECUTE:
                Execute();
                break;
        }
    }

    void Idle()
    {
        agent.speed = fIdleResetSpeed;
        agent.SetDestination(vIdleStartPos);
        
        agentPostion = gameObject.transform.position;
        playerPostion = goPlayer.transform.position;

        if (idleDelayRunning) return; //prevents the agent from going straight to green.

        bool atIdlePos = Vector3.Distance(transform.position, vIdleStartPos) < 0.1f;
        bool playerReady = goPlayer.transform.position.x > tStartPos.position.x;

        if (atIdlePos && playerReady) 
        {
            if (agentPostion.x < playerPostion.x) //Needed for proper reset, probably overdone some other logics, but brain is overwhelmed so this will do for now.
            {
                state = STATE.GREEN;
            }
        }
    }

    void Green()
    {
        agent.speed = fGreenSpeed;
        agent.SetDestination(tGoalPos.position);

    }

    void Yellow()
    {
        agent.speed = fYellowSpeed;
    }

    void Red()
    {
        if (!hasEnteredRedState)
        {
            agent.speed = fRedSpeed;
            vRedStartPos = goPlayer.transform.position;
            hasEnteredRedState = true;

            if (!redDelayRunning)
            {
                StartCoroutine(RedDelay());
            }

            return;
        } 

        if (Vector3.Distance(goPlayer.transform.position, vRedStartPos) > 0.01f)
        {
            CheckPointManager.Instance.RespawnPlayer(goPlayer);

            agent.ResetPath();
            hasEnteredRedState = false;
            Debug.Log("Player has been killed");

            state = STATE.IDLE;

            if (!idleDelayRunning)
            {
                StartCoroutine(IdleDelay()); //Puts on a delay to properly reset the NPC...
            }


        }

        //delay randomrange 2-4sec

        //state = STATE.GREEN;
    }

    void Execute()
    {

    }

    IEnumerator IdleDelay()
    {
        idleDelayRunning = true;

        float delay = CheckPointManager.Instance.fReadRespawnTimer;
        yield return new WaitForSeconds(delay + .5f);

        idleDelayRunning = false;
        state = STATE.IDLE;

    }

    IEnumerator RedDelay()
    {
        redDelayRunning = true;

        float delay = Random.Range(2f, 4f); //makes the anticipation to be different every time.
        yield return new WaitForSeconds(delay);

        hasEnteredRedState = false;
        redDelayRunning = false;
        state = STATE.GREEN;
    }

}
