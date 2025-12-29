using UnityEngine;
using System.Collections;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 8f;
    [Range(0, 360)]
    public float viewAngle = 130f;
    public Transform target;
    public bool canSeePlayer;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    [HideInInspector]
    public Collider[] objectsInView;


    void Start()
    {
        StartCoroutine(FOVtimer());
    }
    private IEnumerator FOVtimer()
    {
        FieldOfViewCheck();
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FOVtimer());
    }

    private void FieldOfViewCheck()
    {
        objectsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        if (objectsInView.Length != 0)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position,
               target.position);
                if (Physics.Raycast(transform.position, directionToTarget, distanceToTarget,
               obstructionMask))
                    canSeePlayer = false;
                else
                    canSeePlayer = true;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }

}