using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (Grid))]
public class Pathfinding : MonoBehaviour {

    private Grid grid;

    private void Awake ()
    {
        grid = GetComponent<Grid> ();
    }

	private void FindPath (Vector3 startPos, Vector3 endPos)
    {
        Node startNode = grid.NodeFromWorldPoint (startPos);
        Node endNode = grid.NodeFromWorldPoint (endPos);

        List<Node> openSet = new List<Node> ();
        HashSet<Node> closedSet = new HashSet<Node> ();

        openSet.Add (startNode);

        while (openSet.Count > 0)
        {
            // Set current to the node in openSet with lowest fCost
            Node currentNode = openSet [0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (
                    openSet [i].fCost < currentNode.fCost || 
                    (openSet [i].fCost == currentNode.fCost && openSet [i].hCost < currentNode.hCost)
                )
                {
                    currentNode = openSet [i];
                }
            }

            openSet.Remove (currentNode);
            closedSet.Add (currentNode);

            if (currentNode == endNode)
            {
                return;
            }

            
            foreach (Node neighbour in grid.GetNeighbours (currentNode))
            {
                // Check if neighbour is traversable or has already been traversed
                if (!neighbour.walkable || closedSet.Contains (neighbour))
                {
                    continue;
                }
            }
        }
    }

}
