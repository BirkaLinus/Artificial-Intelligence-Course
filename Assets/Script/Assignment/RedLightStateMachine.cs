using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class RedLightStateMachine : MonoBehaviour
{

    [Header("References")]
    NavMeshAgent agent;

    [SerializeField] GameObject goPlayer;
    [SerializeField] Transform tStartPos;

    [Header("Vectors3s")]
    [SerializeField] Vector3 vIdleStartPos;
    [SerializeField] Vector3 vRedStartPos; //Store the value to see if the player will survive the redState.
    [SerializeField] Vector3 vPlayerPostion;
    [SerializeField] Vector3 vAgentPostion;
    [SerializeField] Vector3 vGoalPos;

    [Header("Agent Stats")]
    [SerializeField] float fIdleResetSpeed;
    [SerializeField] float fGreenSpeed;
    [SerializeField] float fYellowSpeed;
    [SerializeField] float fRedSpeed;

    [Header("Goal")]
    [SerializeField] float fDestinationX = 201.3f; //The goal destination for the AI and player.

    [Header("Booleans")]
    [SerializeField] bool hasEnteredRedState;
    [SerializeField] bool redDelayRunning;
    [SerializeField] bool idleDelayRunning;
    [SerializeField] bool greenDelayRunning;
    [SerializeField] bool yellowDelayRunning;
    [SerializeField] bool isResetReseted; // 10/10 name, its a check to see if the reset has been reseted to make IDLE work again.
    [SerializeField] bool playerHasReachedGoal;
    [SerializeField] bool atIdlePos;
    [SerializeField] bool playerReady;
    [SerializeField] bool GetThePartyStarted = false;

    [Header("Materials/Renderer/Lights")]
    [SerializeField] Material[] materials;
    [SerializeField] Renderer npcRenderer;
    [SerializeField] GameObject[] goLights;

    public enum STATE { IDLE, GREEN, YELLOW, RED, EXECUTE }
    public STATE state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        //foreach (GameObject light in goLights)
        //{
        //    if (light != null)
        //    {
        //        light.SetActive(false);
        //    }
        //}


        vIdleStartPos = transform.position;
        state = STATE.IDLE;
    }
    void SetMaterial(int index)
    {
        if (materials == null || materials.Length == 0) return;
        if (npcRenderer == null) return;

        index = Mathf.Clamp(index, 0, materials.Length - 1);

        npcRenderer.material = materials[index];
    }

    private void SetActiveLight(int index)
    {
        for (int i = 0; i < goLights.Length; i++)
        {
            if (goLights[i] == null) continue;

            goLights[i].SetActive(i == index);
        }
    }

    private void Update()
    {

        playerHasReachedGoal = goPlayer.transform.position.x >= fDestinationX; //Sets the final destination for AI and player.
        if (playerHasReachedGoal)
        {
            state = STATE.EXECUTE;
        }

        switch (state)
        {
            case STATE.IDLE:
                SetMaterial(0);
                SetActiveLight(-1);
                Idle();
                break;
            case STATE.GREEN:
                SetMaterial(1);
                SetActiveLight(0);
                Green();
                break;
            case STATE.YELLOW:
                SetMaterial(2);
                SetActiveLight(1);
                Yellow();
                break;
            case STATE.RED:
                SetMaterial(3);
                SetActiveLight(2);
                Red();
                break;
                case STATE.EXECUTE:
                Execute();
                break;
        }
    }

    void BoolReset()
    {
        hasEnteredRedState = false;
        redDelayRunning = false;
        idleDelayRunning = false;
        greenDelayRunning = false;
        yellowDelayRunning = false;
    }

    void Idle()
    {

        if (!isResetReseted)
        {
            BoolReset();
            StopAllCoroutines();
            isResetReseted = true;
        }

        agent.speed = fIdleResetSpeed;
        agent.SetDestination(vIdleStartPos);
        
        vAgentPostion = gameObject.transform.position;
        vPlayerPostion = goPlayer.transform.position;

        if (idleDelayRunning) return; //prevents the agent from going straight to green.

        atIdlePos = Vector3.Distance(transform.position, vIdleStartPos) < 0.1f;
        playerReady = goPlayer.transform.position.x < tStartPos.position.x;

        if (atIdlePos && !playerReady)
        {
            if (vAgentPostion.x <= vPlayerPostion.x) //Needed for proper reset, probably overdone some other logics, but brain is overwhelmed so this will do for now.
            {
                state = STATE.GREEN;
                isResetReseted = false;
            }
        }
    }

    void Green()
    {
        agent.speed = fGreenSpeed;

        if (!agent.hasPath) //only need to give the destination once, in this case.
        {
            agent.SetDestination(vGoalPos);
        }


        if (!greenDelayRunning)
        {
            StartCoroutine(GreenDelay());
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            state = STATE.EXECUTE;
        }

    }

    void Yellow()
    {

        vPlayerPostion = goPlayer.transform.position;
        agent.speed = fYellowSpeed;

        if (!yellowDelayRunning)
        {
            StartCoroutine(YellowDelay());
        }

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

    void Execute() //The state when the AI has reached its goal.
    {
        if (!playerHasReachedGoal) //Checks if the player has reached the safespot.
        {
            CheckPointManager.Instance.RespawnPlayer(goPlayer);
            state = STATE.IDLE;
        }
        else
        {
            agent.speed = 0;
            //Destroyed state
            state = STATE.EXECUTE;
            if (!GetThePartyStarted)
            {
                StopAllCoroutines();
                GetThePartyStarted = true;
                StartCoroutine(Party());
            }
        }

    }

    IEnumerator Party()
    {
        SetActiveLight(0);
        yield return new WaitForSeconds(.2f);
        SetActiveLight(1);
        yield return new WaitForSeconds(.2f);
        SetActiveLight(2);
        yield return new WaitForSeconds(.2f);
        StartCoroutine(Party());
    }

    IEnumerator IdleDelay()
    {
        idleDelayRunning = true;

        float delay = CheckPointManager.Instance.fReadRespawnTimer; //Gets the "respawn timer".
        yield return new WaitForSeconds(delay + .5f);

        idleDelayRunning = false;
        state = STATE.IDLE;

    }
    IEnumerator GreenDelay()
    {
        greenDelayRunning = true;

        float delay = Random.Range(3, 6f);
        yield return new WaitForSeconds(delay);

        greenDelayRunning = false;
        state = STATE.YELLOW;
    }

    IEnumerator YellowDelay()
    {
        yellowDelayRunning = true;

        float delay = 1f;

        yield return new WaitForSeconds(delay);

        yellowDelayRunning = false;
        state = STATE.RED;
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
