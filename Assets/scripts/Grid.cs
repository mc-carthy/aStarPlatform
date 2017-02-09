﻿using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }

    public TerrainType[] walkableRegions;

    [SerializeField]
    private LayerMask unwalkableMask;
    [SerializeField]
    private Vector2 gridWorldSize;
    [SerializeField]
    private float nodeRadius;
    [SerializeField]
    private bool displayGridGizmos;
    [SerializeField]
    private int obstacleProximityPenalty;

	private Node [,] grid;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int> ();
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;
    private int penaltyMin = int.MaxValue;
    private int penaltyMax = int.MinValue;

    private void Awake ()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value += region.terrainMask.value;
            walkableRegionsDictionary.Add ((int) Mathf.Log (region.terrainMask.value, 2), region.terrainPenalty);
        }

        CreateGrid ();
    }

    private void OnDrawGizmos ()
    {
        Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.Lerp (Color.white, Color.black, Mathf.InverseLerp (penaltyMin, penaltyMax, n.movementPenalty));
                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
                Gizmos.DrawCube (n.worldPosition, Vector3.one * nodeDiameter);
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

    public List<Node> GetNeighbours (Node node)
    {
        List<Node> neighbours = new List<Node> ();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (
                    checkX > 0 && checkX < gridSizeX &&
                    checkY > 0 && checkY < gridSizeY
                    )
                {
                    neighbours.Add (grid [checkX, checkY]);
                }
            }      
        }

        return neighbours;
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
                int movementPenalty = 0;

                Ray ray = new Ray (worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast (ray, out hit, 100, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue (hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid [x, y] = new Node (walkable, worldPoint, x, y, movementPenalty);
            }   
        }

        BlurPenaltyMap (3);

    }

    private void BlurPenaltyMap (int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        int [,] penaltiesHorizontalPass = new int [gridSizeX, gridSizeY];
        int [,] penaltiesVerticalPass = new int [gridSizeX, gridSizeY];

        // Horizontal pass
        for (int y = 0; y < gridSizeY; y++)
        {
            // First loop for each row
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp (x, 0, kernelExtents);
                penaltiesHorizontalPass [0, y] += grid [sampleX, y].movementPenalty;
            }
            // Remaining loops can make use of removing the value to the left on the new 
            // extents and adding the rightmost value of the new extents
            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp (x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp (x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass [x, y] = 
                    penaltiesHorizontalPass [x - 1, y] - 
                    grid [removeIndex, y].movementPenalty + 
                    grid [addIndex, y].movementPenalty;
            }
        }

        // Vertical pass
        for (int x = 0; x < gridSizeX; x++)
        {
            // First loop for each column
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp (y, 0, kernelExtents);
                penaltiesVerticalPass [x, 0] += penaltiesHorizontalPass [x, sampleY];
            }
            int blurredPenalty = Mathf.RoundToInt ( (float) penaltiesVerticalPass [x, 0] / (kernelSize * kernelSize));
            grid [x, 0].movementPenalty = blurredPenalty;
            
            // Remaining loops can make use of removing the value to the top on the new 
            // extents and adding the bottommost value of the new extents
            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp (y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp (y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass [x, y] = 
                    penaltiesVerticalPass [x, y - 1] - 
                    penaltiesHorizontalPass [x, removeIndex] + 
                    penaltiesHorizontalPass [x, addIndex];

                blurredPenalty = Mathf.RoundToInt ( (float) penaltiesVerticalPass [x, y] / (kernelSize * kernelSize));
                grid [x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
            }
        }
    }

    [System.SerializableAttribute]
    public class TerrainType {
        
        public LayerMask terrainMask;
        public int terrainPenalty;

    }

}
