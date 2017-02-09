using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	public Transform target;

    [SerializeField]
    private float speed = 20f;
    [SerializeField]
    private float turnDistance = 5f;

    private Path path;

    private void Start ()
    {
        PathRequestManager.RequestPath (transform.position, target.position, OnPathFound);
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
            path = new Path (waypoints, transform.position, turnDistance);
            StopCoroutine ("FollowPath");
            StartCoroutine ("FollowPath");
        }
    }

    private IEnumerator FollowPath ()
    {

        while (true)
        {

            yield return null;
        }
    }

}
