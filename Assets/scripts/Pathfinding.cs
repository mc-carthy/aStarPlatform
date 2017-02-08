using UnityEngine;
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

    private void Update ()
    {
        if (Input.GetButtonDown ("Jump"))
        {
            FindPath (seeker.position, target.position);
        }
    }

	private void FindPath (Vector3 startPos, Vector3 endPos)
    {
        Stopwatch sw = new Stopwatch ();
        sw.Start ();

        Node startNode = grid.NodeFromWorldPoint (startPos);
        Node endNode = grid.NodeFromWorldPoint (endPos);

        Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node> ();

        openSet.Add (startNode);

        while (openSet.Count > 0)
        {
            // Set current to the node in openSet with lowest fCost
            Node currentNode = openSet.RemoveFirstItemFromHeap ();
            // ====== Removed when switching from List to Heap data structure ========

            // Node currentNode = openSet [0];
            // for (int i = 1; i < openSet.Count; i++)
            // {
            //     if (
            //         openSet [i].fCost < currentNode.fCost || 
            //         (openSet [i].fCost == currentNode.fCost && openSet [i].hCost < currentNode.hCost)
            //     )
            //     {
            //         currentNode = openSet [i];
            //     }
            // }

            // openSet.Remove (currentNode);
            closedSet.Add (currentNode);

            if (currentNode == endNode)
            {
                sw.Stop ();
                print ("Path found in " + sw.ElapsedMilliseconds + "ms");
                RetracePath (startNode, endNode);
                return;
            }

            
            foreach (Node neighbour in grid.GetNeighbours (currentNode))
            {
                // Check if neighbour is traversable or has already been traversed
                if (!neighbour.walkable || closedSet.Contains (neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour);

                // If new path to neighbour is shorter or neighbour is not in openSet
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance (neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains (neighbour))
                    {
                        openSet.Add (neighbour);
                        openSet.UpdateItem (neighbour);
                    }
                }
            }
        }
    }

    private void RetracePath (Node startNode, Node endNode)
    {
        List<Node> path = new List<Node> ();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add (currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse ();

        grid.path = path;

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
