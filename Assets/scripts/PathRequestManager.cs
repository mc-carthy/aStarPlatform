using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public class PathRequestManager : MonoBehaviour {

    static PathRequestManager Instance;

    private Queue<PathResult> results = new Queue<PathResult> ();
    private Pathfinding pathfinding;

    private void Awake ()
    {
        Instance = this;
        pathfinding = GetComponent<Pathfinding> ();
    }

    private void Update ()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue ();
                    result.callback (result.path, result.success);
                }
            }
        }
    }

	public static void RequestPath (PathRequest request)
    {
        ThreadStart threadStart = delegate {
            Instance.pathfinding.FindPath (request, Instance.FinishedProcessingPath);
        };
        threadStart.Invoke ();
    }

    public void FinishedProcessingPath (PathResult result)
    {
        lock (results)
        {
            results.Enqueue (result);
        }
    }

}

public struct PathResult {
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult (Vector3[] _path, bool _success, Action<Vector3[], bool> _callback)
    {
        path = _path;
        success = _success;
        callback = _callback;
    }
}

public struct PathRequest {
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