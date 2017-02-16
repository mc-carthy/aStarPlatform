using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    const float timePathUpdateThreshold = 0.2f;
    const float targetMovePathUpdateThreshold = 0.5f;

	public Transform target;

    [SerializeField]
    private float speed = 20f;
    [SerializeField]
    private float turnDistance = 5f;
    [SerializeField]
    private float turnSpeed = 3f;
    [SerializeField]
    private float stoppingDistance = 500f;

    private Path path;

    private void Start ()
    {
        StartCoroutine ("UpdatePath");
    }

    private void OnDrawGizmos ()
    {
        if (path != null)
        {
            path.DrawWithGizmos ();
        }
    }

    public void OnPathFound (Vector3[] waypoints, bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = new Path (waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine ("FollowPath");
            StartCoroutine ("FollowPath");
        }
    }

    private IEnumerator UpdatePath ()
    {
        if (Time.timeSinceLevelLoad < 0.3f)
        {
            yield return new WaitForSeconds (0.3f);
        }
        PathRequestManager.RequestPath (new PathRequest (transform.position, target.position, OnPathFound));
        float squareMoveThreshold = targetMovePathUpdateThreshold * targetMovePathUpdateThreshold;
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds (timePathUpdateThreshold);
            if ((target.position - targetPosOld).sqrMagnitude > squareMoveThreshold)
            {
                PathRequestManager.RequestPath (new PathRequest (transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }

    private IEnumerator FollowPath ()
    {
        bool isFollowingPath = true;
        int pathIndex = 0;
        transform.LookAt (path.lookPoints [0]);

        float speedPercent = 1;

        while (isFollowingPath)
        {
            Vector2 pos2D = new Vector2 (transform.position.x, transform.position.z);
            while (path.turnBoundaries [pathIndex].HasCrossedLine (pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    isFollowingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (isFollowingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01 (path.turnBoundaries [path.finishLineIndex].DistanceFromPoint (pos2D) / stoppingDistance);
                    if (speedPercent < 0.01f)
                    {
                        isFollowingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation (path.lookPoints [pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate (Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;
        }
    }

}
