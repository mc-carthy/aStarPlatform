using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour {

    public float viewRadius;
    [RangeAttribute (0, 360)]
    public float viewAngle;
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform> ();

    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;

    private void Start ()
    {
        StartCoroutine ("FindTargetsWithDelay", 0.2f);
    }

    public Vector3 DirFromAngle (float angleInDegrees, bool isGlobalAngle)
    {
        if (!isGlobalAngle)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3 (Mathf.Sin (angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos (angleInDegrees * Mathf.Deg2Rad));
    }

    private IEnumerator FindTargetsWithDelay (float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds (delay);
            FindVisibleTargets ();
        }
    }

    private void FindVisibleTargets ()
    {
        visibleTargets.Clear ();
        Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius [i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance (transform.position, target.position);
                if (!Physics.Raycast (transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add (target);
                }
            }
        }
    }

}
