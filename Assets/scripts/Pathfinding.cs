using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[RequireComponent (typeof (Grid))]
public class Pathfinding : MonoBehaviour {

    public Transform seeker;
    public Transform target;

    private Grid grid;

    private void Awake ()
    {
        grid = GetComponent<Grid> ();
    }

	public void FindPath (PathRequest request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch ();
        sw.Start ();

        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint (request.pathStart);
        Node endNode = grid.NodeFromWorldPoint (request.pathEnd);

        if (startNode.walkable && endNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node> ();

            openSet.Add (startNode);

            while (openSet.Count > 0)
            {
                // Set current to the node in openSet with lowest fCost
                Node currentNode = openSet.RemoveFirstItemFromHeap ();

                closedSet.Add (currentNode);

                if (currentNode == endNode)
                {
                    sw.Stop ();
                    // print ("Path found in " + sw.ElapsedMilliseconds + "ms");
                    pathSuccess = true;
                    break;
                }

                
                foreach (Node neighbour in grid.GetNeighbours (currentNode))
                {
                    // Check if neighbour is traversable or has already been traversed
                    if (!neighbour.walkable || closedSet.Contains (neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour) + neighbour.movementPenalty;

                    // If new path to neighbour is shorter or neighbour is not in openSet
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance (neighbour, endNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains (neighbour))
                        {
                            openSet.Add (neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem (neighbour);
                        }
                    }
                }
            }
        }
        if (pathSuccess)
        {
            wayPoints = RetracePath (startNode, endNode);
            pathSuccess = wayPoints.Length > 0;
        }
        callback (new PathResult (wayPoints, pathSuccess, request.callback));
    }

    private Vector3[] RetracePath (Node startNode, Node endNode)
    {
        List<Node> path = new List<Node> ();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add (currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath (path);
        Array.Reverse (waypoints);
        return waypoints;
    }

    private Vector3[] SimplifyPath (List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3> ();
        Vector2 dirOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 dirNew = new Vector2 (path [i - 1].gridX - path [i].gridX, path [i - 1].gridY - path [i].gridY);
            if (dirNew != dirOld)
            {
                waypoints.Add (path [i].worldPosition);
            }
            dirOld = dirNew;
        }

        return waypoints.ToArray ();
    }

    private int GetDistance (Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }

}
