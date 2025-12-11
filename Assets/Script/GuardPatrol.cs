using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Lab1
{
    public class GuardPatrol : MonoBehaviour
    {
        public Transform[] wayPoints;
        public float wayPointTolerance = 0.5f;

        int _currentIndex = 0;
        NavMeshAgent _agent;


        [SerializeField] Material[] materials;
        [SerializeField] Renderer npcRenderer;
        public enum GuardState
        {
            Patrolling,
            Chasing,
            Attack,
            ReturningToPatrol
        }
        public GuardState currentstate = GuardState.Patrolling;
        [SerializeField] Transform player;
        [SerializeField] float fChaseRange = 5f;
        [SerializeField] float fLoseRange = 7f;
        [SerializeField] float fAttackRange = 1f;


        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (wayPoints.Length > 0)
            {
                _agent.SetDestination(wayPoints[_currentIndex].position);
            }
        }

        private void Update()
        {
            switch (currentstate)
            {
                case GuardState.Patrolling:
                    SetMaterial(0);
                    Patrol();
                    break;
                case GuardState.Chasing:
                    SetMaterial(1);
                    Chase();
                    break;
                case GuardState.Attack:
                    AttackPlayer();
                    break;
                case GuardState.ReturningToPatrol:
                    SetMaterial(0);
                    LostPlayer();
                    break;

            }
        }

        void SetMaterial(int index)
        {
            if (materials == null || materials.Length == 0) return;
            if (npcRenderer == null) return;

            index = Mathf.Clamp(index, 0, materials.Length - 1);

            npcRenderer.material = materials[index];
        }

        void Patrol()
        {
            if (wayPoints.Length == 0) return;

            if (!_agent.pathPending && _agent.remainingDistance <= wayPointTolerance)
            {
                _currentIndex = (_currentIndex + 1) % wayPoints.Length;
                _agent.SetDestination(wayPoints[_currentIndex].position);
            }
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= fChaseRange)
            {
                currentstate = GuardState.Chasing;
            }
        }

        void Chase()
        {
            _agent.SetDestination(player.position);

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer >= fLoseRange)
            {
                currentstate = GuardState.ReturningToPatrol;
            }
            if (distanceToPlayer <= fAttackRange)
            {
                currentstate = GuardState.Attack;
            }
        }

        void AttackPlayer()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float fStopOffset = 1.5f;

            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            Vector3 targetPosition = player.position - directionToPlayer * fStopOffset;

            _agent.SetDestination(targetPosition);

            if (distanceToPlayer > fStopOffset + .5f)
            {
                currentstate = GuardState.Chasing;
            }
        }

        void LostPlayer()
        {
            _currentIndex = GetClosestWayPointIndex();
            _agent.SetDestination(wayPoints[_currentIndex].position);
            currentstate = GuardState.Patrolling;
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
    }

}


