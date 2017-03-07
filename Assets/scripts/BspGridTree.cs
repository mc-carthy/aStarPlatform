using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BspGridTree : MonoBehaviour {

    [SerializeField]
    private Vector2 treeSize;

    private BspGridLeaf root;
    private List<BspGridLeaf> leaves = new List<BspGridLeaf> ();
	public int[,] grid;

        
    private BspGridLeaf exitLeafMaxX;
    private BspGridLeaf exitLeafMaxY;
    private BspGridLeaf exitLeafMinX;
    private BspGridLeaf exitLeafMinY;

    private void Start ()
    {
        CreateRooms (treeSize);
        BuildExit ();
        BuildWalls ();
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
        }
    }

    private void CreateRooms (Vector2 size)
    {
        root = new BspGridLeaf (0, 0, (int) size.x, (int) size.y, null);
        leaves.Add (root);

        bool didSplit = true;
        while (didSplit)
        {
            didSplit = false;

            for (int i = 0; i < leaves.Count; i++)
            {
                // If we haven't already split the leaf
                if (leaves [i].firstChild == null && leaves [i].secondChild == null)
                {
                    // Attempt to split it
                    if (leaves [i].Split ())
                    {
                        // If we can split it, add its children to the leaves list
                        leaves.Add (leaves [i].firstChild);
                        leaves.Add (leaves [i].secondChild);
                        
                        didSplit = true;
                    }
                }
            }

            root.CreateRooms ();
        }

        SetGrid ();
        CreateCorridors ();
    }

    private void CreateCorridors ()
    {
        foreach (BspGridLeaf leaf in leaves)
        {
            if (leaf.parent != null)
            {
                if (leaf.parent.firstChild != null && leaf.parent.secondChild != null)
                {
                    if (leaf.parent.firstChild.hasRoom && leaf.parent.secondChild.hasRoom)
                    {
                        // Debug.DrawLine (leaf.parent.firstChild.room.centrePos, leaf.parent.secondChild.room.centrePos, Color.red, 2f);
                        int x = (int) leaf.parent.firstChild.room.centrePos.x;
                        int y = (int) leaf.parent.firstChild.room.centrePos.y;
                        int targetX = (int) leaf.parent.secondChild.room.centrePos.x;
                        int targetY = (int) leaf.parent.secondChild.room.centrePos.y;

                        while (x != targetX)
                        {
                            grid [x, y] = 0;
                            x += x < targetX ? 1 : -1;
                        }

                        while (y != targetY)
                        {
                            grid [x, y] = 0;
                            y += y < targetY ? 1 : -1;
                        }

                    }
                    else
                    {
                        // Debug.DrawLine (leaf.parent.firstChild.centre, leaf.parent.secondChild.centre, Color.green, 2f);
                        int x = (int) leaf.parent.firstChild.centre.x;
                        int y = (int) leaf.parent.firstChild.centre.y;
                        int targetX = (int) leaf.parent.secondChild.centre.x;
                        int targetY = (int) leaf.parent.secondChild.centre.y;

                        int dX = x < targetX ? 1 : -1;
                        int dY = y < targetY? 1 : -1;

                        if (x != targetX)
                        {
                            while (x >= 0 && x < treeSize.x)
                            // while (x != targetX)
                            {
                                grid [x, y] = 0;
                                // x += x < targetX ? 1 : -1;
                                x += dX;
                            }
                        }

                        if (y != targetY)
                        {
                            while (y >= 0 && y < treeSize.y)
                            // while (y != targetY)
                            {
                                grid [x, y] = 0;
                                // y += y < targetY ? 1 : -1;
                                y += dY;
                            }
                        }

                    }

                }

            }
        }
    }

    private void BuildExit ()
    {

        // Randomly choose an edge to exit at
        // float ran = Random.Range (0f, 1f);
        // if (ran <= 0.25f)
        // {
        // }
        // else if (ran <= 0.5f)
        // {
        // }
        // else if (ran <= 0.75f)
        // {
        // }
        // else
        // {
        // }

        foreach (BspGridLeaf leaf in leaves)
        {
            if (leaf.hasRoom)
            {
                exitLeafMaxX = leaf;
                exitLeafMaxY = leaf;
                exitLeafMinX = leaf;
                exitLeafMinY = leaf;
                break;
            }
        }

        foreach (BspGridLeaf leaf in leaves)
        {
            if (leaf.hasRoom)
            {
                if (leaf.room.centrePos.x > exitLeafMaxX.room.centrePos.x)
                {
                    exitLeafMaxX = leaf;
                }
                if (leaf.room.centrePos.y > exitLeafMaxY.room.centrePos.y)
                {
                    exitLeafMaxY = leaf;
                }
                if (leaf.room.centrePos.x < exitLeafMinX.room.centrePos.x)
                {
                    exitLeafMinX = leaf;
                }
                if (leaf.room.centrePos.y < exitLeafMinY.room.centrePos.y)
                {
                    exitLeafMinY = leaf;
                }
            }
        }
        // Debug.DrawLine (new Vector2 (treeSize.x / 2f, treeSize.y / 2f), exitLeafMaxX.room.centrePos, Color.red, 5f);
        // Debug.DrawLine (new Vector2 (treeSize.x / 2f, treeSize.y / 2f), exitLeafMaxY.room.centrePos, Color.red, 5f);
        // Debug.DrawLine (new Vector2 (treeSize.x / 2f, treeSize.y / 2f), exitLeafMinX.room.centrePos, Color.red, 5f);
        // Debug.DrawLine (new Vector2 (treeSize.x / 2f, treeSize.y / 2f), exitLeafMinY.room.centrePos, Color.red, 5f);
    }

    private void SetGrid ()
    {
        grid = new int [(int) treeSize.x, (int) treeSize.y];

        for (int x = 0; x < treeSize.x; x++)
        {
            for (int y = 0; y < treeSize.y; y++)
            {
                grid [x, y] = 1;
            }
        }

        foreach (BspGridLeaf leaf in leaves)
        {
            if (leaf.hasRoom)
            {
                for (int x = 0; x < (int) leaf.room.size.x; x++)
                {
                    for (int y = 0; y < (int) leaf.room.size.y; y++)
                    {
                        int gridX = x + (int) leaf.room.bottomLeft.x;
                        int gridY = y + (int) leaf.room.bottomLeft.y;
                        grid [gridX, gridY] = 0;
                    }
                }
            }
        }
    }

    private void BuildWalls ()
    {
        for (int x = 0; x < treeSize.x; x++)
        {
            for (int y = 0; y < treeSize.y; y++)
            {
                if (grid [x, y] == 1)
                {
                    GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
                    cube.transform.position = new Vector2 (x, y);
                    cube.transform.parent = transform;
                    cube.GetComponent<Renderer> ().sharedMaterial.color = Color.black;
                }
            }
        }
    }

    // private void OnDrawGizmos ()
    // {
    //     for (int x = 0; x < treeSize.x; x++)
    //     {
    //         for (int y = 0; y < treeSize.y; y++)
    //         {
    //             Gizmos.color = (grid [x, y] == 1) ? Color.black : Color.white;
    //             Gizmos.DrawCube (new Vector3 (x, y, 0), Vector3.one * 0.75f);
    //         }
    //     }
    // }

}
