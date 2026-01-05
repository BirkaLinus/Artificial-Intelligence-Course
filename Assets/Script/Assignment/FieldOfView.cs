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
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if target is in range
        if (distanceToTarget <= viewRadius)
        {
            //checks if target is in the "correct" angle
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                //checking if its not an obstruction in the way of the target.
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }

        // If any check fails
        canSeePlayer = false;
    }

}