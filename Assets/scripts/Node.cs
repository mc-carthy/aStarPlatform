using UnityEngine;

public class Node : IHeapItem<Node> {

    private int heapIndex;
    public int HeapIndex {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    public Node parent;
	public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public Node (bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int CompareTo (Node nodeToCompare)
    {
        int compare = fCost.CompareTo (nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo (nodeToCompare.hCost);
        }
        // We return negative compare because a lower fCost is better
        return -compare;
    }

}
