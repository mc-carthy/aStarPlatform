using UnityEngine;
using System;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour {

    static PathRequestManager Instance;

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest> ();
    private PathRequest currentPathRequest;
    private Pathfinding pathfinding;
    private bool isProcessingPath;

    private void Awake ()
    {
        Instance = this;
        pathfinding = GetComponent<Pathfinding> ();
    }

	public static void RequestPath (Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest (pathStart, pathEnd, callback);
        Instance.pathRequestQueue.Enqueue (newRequest);
        Instance.TryProcessNext ();
    }

    public void FinishedProcessingPath (Vector3[] path, bool success)
    {
        currentPathRequest.callback (path, success);
        isProcessingPath = false;
        TryProcessNext ();
    }

    private void TryProcessNext ()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue ();
            isProcessingPath = true;
            pathfinding.StartFindPath (currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    struct PathRequest {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest (Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[], bool> _callback)
        {
            pathStart = _pathStart;
            pathEnd = _pathEnd;
            callback = _callback;
        }
    }
}
