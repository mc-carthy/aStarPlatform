using UnityEngine;

public class Grid : MonoBehaviour {

    [SerializeField]
    private LayerMask unwalkableMask;
    [SerializeField]
    private Vector2 gridWorldSize;
    [SerializeField]
    private float nodeRadius;

	private Node [,] grid;
    private Transform player;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    private void Awake ()
    {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);
    }

    private void Start ()
    {
        CreateGrid ();
    }

    private void OnDrawGizmos ()
    {
        Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            Node playerNode = NodeFromWorldPoint (player.position);

            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (playerNode == n)
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawCube (n.worldPosition, Vector3.one * nodeDiameter * 0.9f);
            }
        }
    }

    public Node NodeFromWorldPoint (Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2f) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2f) / gridWorldSize.y;
        percentX = Mathf.Clamp01 (percentX);
        percentY = Mathf.Clamp01 (percentY);

        int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);

        return grid [x, y];
    }

    private void CreateGrid ()
    {
        grid = new Node [gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = 
                    worldBottomLeft + 
                    Vector3.right * (x * nodeDiameter + nodeRadius) + 
                    Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                bool walkable = !( Physics.CheckSphere (worldPoint, nodeRadius, unwalkableMask));
                grid [x, y] = new Node (walkable, worldPoint);
            }   
        }
    }

}
