using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BspGridTree : MonoBehaviour {

    [SerializeField]
    private Vector2 treeSize;

    private BspGridLeaf root;
    private List<BspGridLeaf> leaves = new List<BspGridLeaf> ();
	public int[,] grid;

    private void Start ()
    {
        CreateRooms ();
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
        }
    }

    private void CreateRooms ()
    {
        root = new BspGridLeaf (0, 0, (int) treeSize.x, (int) treeSize.y, null);
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
                        Debug.Log ("x : " + leaf.room.bottomLeft.x + (leaf.room.size.x / 2));
                        Debug.Log ("y : " + leaf.room.bottomLeft.y + (leaf.room.size.y / 2));
                    }
                }
            }
        }
    }

    private void OnDrawGizmos ()
    {
        for (int x = 0; x < treeSize.x; x++)
        {
            for (int y = 0; y < treeSize.y; y++)
            {
                Gizmos.color = (grid [x, y] == 1) ? Color.black : Color.white;
                Gizmos.DrawCube (new Vector3 (x, y, 0), Vector3.one * 0.75f);
            }
        }
    }

}
