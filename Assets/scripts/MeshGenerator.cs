using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

	public SquareGrid squareGrid;

	[SerializeField]
	private MeshFilter walls;
	[SerializeField]
	private MeshFilter cave;
	[SerializeField]
	private Material wallMat;
	[SerializeField]
	private Material caveMat;
	[SerializeField]
	private bool is2D;
	private float CameraHeight2D = -10f;

	private float CameraHeight3D = 70f;

	private List<Vector3> vertices;
	private List<int> triangles;

	// Return a list of triangles that a vertex belongs to
	private Dictionary<int,List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
	// Edge outlines
	private List<List<int>> outlines = new List<List<int>> ();
	// These vertices have already been checked to see if they're part of an outline
	private HashSet<int> checkedVertices = new HashSet<int> ();

	private float wallHeight = 5f;

	// private void OnDrawGizmos ()
	// {
	// 	if (squareGrid != null)
	// 	{
	// 		for (int x = 0; x < squareGrid.squares.GetLength (0); x ++)
	// 		{
	// 			for (int y = 0; y < squareGrid.squares.GetLength (1); y ++)
	// 			{
	// 				Gizmos.color = (squareGrid.squares [x, y].topLeft.active) ? Color.black : Color.white;
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].topLeft.position, Vector3.one * 0.5f);

	// 				Gizmos.color = (squareGrid.squares [x, y].topRight.active) ? Color.black : Color.white;
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].topRight.position, Vector3.one * 0.5f);

	// 				Gizmos.color = (squareGrid.squares [x, y].bottomRight.active) ? Color.black : Color.white;
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].bottomRight.position, Vector3.one * 0.5f);

	// 				Gizmos.color = (squareGrid.squares [x, y].bottomLeft.active) ? Color.black : Color.white;
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].bottomLeft.position, Vector3.one * 0.5f);

	// 				Gizmos.color = Color.grey;
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].centreTop.position, Vector3.one * 0.15f);
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].centreRight.position, Vector3.one * 0.15f);
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].centreBottom.position, Vector3.one * 0.15f);
	// 				Gizmos.DrawCube (squareGrid.squares [x, y].centreLeft.position, Vector3.one * 0.15f);
	// 			}
	// 		}
	// 	}
	// }

	public void GenerateMesh (int[,] map, float squareSize) {

		triangleDictionary.Clear ();
		outlines.Clear ();
		checkedVertices.Clear ();

		squareGrid = new SquareGrid (map, squareSize);

		vertices = new List<Vector3> ();
		triangles = new List<int> ();

		for (int x = 0; x < squareGrid.squares.GetLength (0); x++)
		{
			for (int y = 0; y < squareGrid.squares.GetLength (1); y++)
			{
				TriangulateSquare (squareGrid.squares [x, y]);
			}
		}

		Mesh mesh = new Mesh ();
		cave.mesh = mesh;
		cave.GetComponent<MeshRenderer> ().material = caveMat;

		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();


		// Number of times this texture will tile across the mesh
		int tileAmount = 10;
		Vector2 [] uvs = new Vector2 [vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			float percentX = Mathf.InverseLerp (-map.GetLength (0) / 2 * squareSize, map.GetLength (0) / 2 * squareSize, vertices [i].x) * tileAmount;
			float percentY = Mathf.InverseLerp (-map.GetLength (0) / 2 * squareSize, map.GetLength (0) / 2 * squareSize, vertices [i].z) * tileAmount;
			uvs [i] = new Vector2 (percentX, percentY);
		}
		mesh.uv = uvs;
	

		if (is2D)
		{
			Camera mainCam = Camera.main;
			mainCam.transform.rotation = Quaternion.identity;
			mainCam.transform.position = new Vector3 (0f, 0f, CameraHeight2D);
			mainCam.orthographic = true;
			// TODO
			mainCam.orthographicSize = 45;
			
			Generate2DColliders ();
			cave.transform.rotation = Quaternion.Euler (270f, 0f, 0f);
		}
		else
		{
			Camera mainCam = Camera.main;
			mainCam.transform.rotation = Quaternion.Euler (90f, 0, 0);
			mainCam.transform.position = new Vector3 (0f, CameraHeight3D, 0f);
			mainCam.orthographic = false;
			// TODO
			mainCam.fieldOfView = 60f;
			CreateWallMesh ();
		}
	}

	private void CreateWallMesh () {

		MeshCollider currentCollider = GetComponent<MeshCollider> ();
		Destroy (currentCollider);

		CalculateMeshOutlines ();

		List<Vector3> wallVertices = new List<Vector3> ();
		List<int> wallTriangles = new List<int> ();
		Mesh wallMesh = new Mesh ();

		foreach (List<int> outline in outlines)
		{
			for (int i = 0; i < outline.Count - 1; i++)
			{
				int startIndex = wallVertices.Count;
				wallVertices.Add (vertices [outline [i]]); // Left
				wallVertices.Add (vertices [outline [i + 1]]); // Right
				wallVertices.Add (vertices [outline [i]] - Vector3.up * wallHeight); // Bottom left
				wallVertices.Add (vertices [outline [i + 1]] - Vector3.up * wallHeight); // Bottom right

				wallTriangles.Add (startIndex + 0);
				wallTriangles.Add (startIndex + 2);
				wallTriangles.Add (startIndex + 3);

				wallTriangles.Add (startIndex + 3);
				wallTriangles.Add (startIndex + 1);
				wallTriangles.Add (startIndex + 0);
			}
		}

		wallMesh.vertices = wallVertices.ToArray ();
		wallMesh.triangles = wallTriangles.ToArray ();
		walls.mesh = wallMesh;
		walls.GetComponent<MeshRenderer> ().material = wallMat;


		MeshCollider wallCollider = gameObject.AddComponent<MeshCollider> ();
		wallCollider.sharedMesh = wallMesh;
	}

	private void Generate2DColliders () {

		// Remove any pre-existing colliders, in the event of regenerating the map
		EdgeCollider2D [] currentColliders = gameObject.GetComponents<EdgeCollider2D> ();
		for (int i = 0; i < currentColliders.Length; i++)
		{
			Destroy (currentColliders [i]);
		}

		CalculateMeshOutlines ();

		foreach (List<int> outline in outlines)
		{
			EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
			Vector2 [] edgePoints = new Vector2 [outline.Count];

			for (int i = 0; i < outline.Count; i ++)
			{
				edgePoints [i] = new Vector2 (vertices [outline [i]].x, vertices [outline [i]].z);
			}
			edgeCollider.points = edgePoints;
		}

	}

	private void TriangulateSquare (Square square) {
		switch (square.configuration) {
		
		// Empty square
		case 0:
			break;

		// 1 point:

		// Bottom Left
		case 1:
			MeshFromPoints (square.centreLeft, square.centreBottom, square.bottomLeft);
			break;
		// Bottom Right
		case 2:
			MeshFromPoints (square.bottomRight, square.centreBottom, square.centreRight);
			break;
		// Top Right
		case 4:
			MeshFromPoints (square.topRight, square.centreRight, square.centreTop);
			break;
		// Top Left
		case 8:
			MeshFromPoints (square.topLeft, square.centreTop, square.centreLeft);
			break;

		// 2 points (same side):

		// Bottom
		case 3:
			MeshFromPoints (square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		// Right
		case 6:
			MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
			break;
		// Left
		case 9:
			MeshFromPoints (square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
			break;
		// Top
		case 12:
			MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreLeft);
			break;

		// 2 points (opposing sides)

		// Top Right - Bottom Left
		case 5:
			MeshFromPoints (square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
			break;
		// Top Left - Bottom Right
		case 10:
			MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;

		// 3 points:

		// Not Top Left
		case 7:
			MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		// Not Top Right
		case 11:
			MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
			break;
		// Not Bottom Right
		case 13:
			MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
			break;
		// Not Bottom Left
		case 14:
			MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;

		// 4 points:

		// Full Square
		case 15:
			MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);

			// All surrounding points are walls, dont bother checking if they're part of an outline edge
			checkedVertices.Add (square.topLeft.vertexIndex);
			checkedVertices.Add (square.topRight.vertexIndex);
			checkedVertices.Add (square.bottomRight.vertexIndex);
			checkedVertices.Add (square.bottomLeft.vertexIndex);

			break;
		}

	}

	private void MeshFromPoints (params Node[] points)
	{
		AssignVertices (points);

		if (points.Length >= 3)
			CreateTriangle (points[0], points[1], points[2]);
		if (points.Length >= 4)
			CreateTriangle (points[0], points[2], points[3]);
		if (points.Length >= 5) 
			CreateTriangle (points[0], points[3], points[4]);
		if (points.Length >= 6)
			CreateTriangle (points[0], points[4], points[5]);

	}

	private void AssignVertices (Node[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			// If we haven't already assigned this point
			if (points [i].vertexIndex == -1)
			{
				// Assign this point the next available vertex index
				points [i].vertexIndex = vertices.Count;
				// Add this point to the vertices list
				vertices.Add (points [i].position);
			}
		}
	}

	private void CreateTriangle (Node a, Node b, Node c)
	{
		triangles.Add (a.vertexIndex);
		triangles.Add (b.vertexIndex);
		triangles.Add (c.vertexIndex);

		Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);

		AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
	}

	private void AddTriangleToDictionary (int vertexIndexKey, Triangle triangle)
	{
		// If the vertex is already present in triangleDictionary, add this triangle to its list of triangles
		if (triangleDictionary.ContainsKey (vertexIndexKey))
		{
			triangleDictionary [vertexIndexKey].Add (triangle);
		}
		// Else create a new list for this vertex and add this triangle to it
		else
		{
			List<Triangle> triangleList = new List<Triangle> ();
			triangleList.Add (triangle);
			triangleDictionary.Add (vertexIndexKey, triangleList);
		}
	}

	// Go through every vertex and test if it's part of an outline edge
	// If so, follow the outline until it meets up with itself
	private void CalculateMeshOutlines ()
	{
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
		{
			if (!checkedVertices.Contains (vertexIndex))
			{
				int newOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
				// If this is an outline edge vertex
				if (newOutlineVertex != -1)
				{
					checkedVertices.Add (vertexIndex);

					List<int> newOutline = new List<int> ();
					newOutline.Add (vertexIndex);
					outlines.Add (newOutline);

					// Follow the connected edges of this outline all the way around and add it to the outlines list
					FollowOutline (newOutlineVertex, outlines.Count - 1);
					outlines [outlines.Count - 1].Add (vertexIndex);
				}
			}
		}

		SimplifyMeshOutlines ();
	}

	// Remove vertices that lie on the line between the previous and next vertex
	private void SimplifyMeshOutlines ()
	{
		for (int outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex++)
		{
			List<int> simplifiedOutline = new List<int> ();
			Vector3 dirOld = Vector3.zero;
			for (int i = 0; i < outlines [outlineIndex].Count; i++)
			{
				Vector3 p1 = vertices [outlines [outlineIndex] [i]];
				Vector3 p2 = vertices [outlines [outlineIndex] [(i + 1) % outlines [outlineIndex].Count]];
				Vector3 dir = p1 - p2;
				if (dir != dirOld)
				{
					dirOld = dir;
					simplifiedOutline.Add (outlines [outlineIndex] [i]);
				}
			}
			outlines [outlineIndex] = simplifiedOutline;
		}
	}

	// Follow the given edge vertex all the way around the outline adding them as we go until we get back to where we entered and complete the loop
	private void FollowOutline (int vertexIndex, int outlineIndex)
	{
		outlines [outlineIndex].Add (vertexIndex);
		checkedVertices.Add (vertexIndex);
		int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);

		if (nextVertexIndex != -1)
		{
			FollowOutline (nextVertexIndex, outlineIndex);
		}
	}

	// Returns the vertex with which the supplied vertex shares an outline edge
	// Returns -1 if it is does not share an edge or if we've already checked the connecting edge
	// If the latter occurs, it means we've followed the outline all the way around to the first vertex of the outline
	private int GetConnectedOutlineVertex (int vertexIndex)
	{
		List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];

		for (int i = 0; i < trianglesContainingVertex.Count; i++)
		{
			Triangle triangle = trianglesContainingVertex [i];

			// Check the 3 vertices of the supplied triangle
			for (int j = 0; j < 3; j++)
			{
				int vertexB = triangle [j];
				if (vertexB != vertexIndex && !checkedVertices.Contains (vertexB))
				{
					if (IsOutlineEdge (vertexIndex, vertexB))
					{
						return vertexB;
					}
				}
			}
		}

		return -1;
	}

	// Check if edge connecting two vertices is going to be a wall edge
	// If the two vertices making up the edge only share one triangle, it is an outline edge that requires a wall
	private bool IsOutlineEdge (int vertexA, int vertexB)
	{
		List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
		int sharedTriangleCount = 0;

		for (int i = 0; i < trianglesContainingVertexA.Count; i++)
		{
			if (trianglesContainingVertexA [i].Contains (vertexB))
			{
				sharedTriangleCount ++;
				if (sharedTriangleCount > 1)
				{
					break;
				}
			}
		}
		return sharedTriangleCount == 1;
	}

	struct Triangle {
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;
		int [] vertices;

		public Triangle (int a, int b, int c)
		{
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;

			vertices = new int [3];
			vertices [0] = a;
			vertices [1] = b;
			vertices [2] = c;
		}

		// Indexer used to return this triangles vertices
		public int this [int i] {
			get {
				return vertices [i];
			}
		}


		public bool Contains (int vertexIndex)
		{
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
	}

	// This class holds a 2D array of squares
	// This takes in the MapGenerator.map and creates the marching squares mesh
	public class SquareGrid {
		public Square [,] squares;

		public SquareGrid(int [,] map, float squareSize)
		{
			int nodeCountX = map.GetLength (0);
			int nodeCountY = map.GetLength (1);
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;

			ControlNode [,] controlNodes = new ControlNode [nodeCountX, nodeCountY];

			for (int x = 0; x < nodeCountX; x ++)
			{
				for (int y = 0; y < nodeCountY; y ++)
				{
					Vector3 pos = new Vector3(
						-mapWidth / 2 + x * squareSize + squareSize / 2,
						0,
						-mapHeight / 2 + y * squareSize + squareSize / 2
					);
					controlNodes [x, y] = new ControlNode (pos, map [x, y] == 1, squareSize);
				}
			}

			squares = new Square [nodeCountX - 1, nodeCountY - 1];
			for (int x = 0; x < nodeCountX - 1; x ++)
			{
				for (int y = 0; y < nodeCountY - 1; y ++)
				{
					squares [x, y] = new Square (controlNodes [x, y + 1], controlNodes [x + 1, y + 1], controlNodes [x + 1, y], controlNodes [x, y]);
				}
			}

		}
	}
	
	// A square is bounded by its 4 ControlNodes with a Node at the midpoint of each edge
	// This is filled with a mesh corresponding to one of the 16 variants outlined in the TriangulateSquare method
	// Which mesh is chosen is dependant on the configuration variable which is dependant on the active status of its ControlNodes
	// Configuration is a 4 bit number where the bits are represented by the active state of the 4 corners in the order of the constructor
	public class Square {

		public ControlNode topLeft, topRight, bottomRight, bottomLeft;
		public Node centreTop, centreRight, centreBottom, centreLeft;
		public int configuration;

		public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;

			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;

			if (topLeft.active)
			{
				configuration += 8;
			}
			if (topRight.active)
			{
				configuration += 4;
			}
			if (bottomRight.active)
			{
				configuration += 2;
			}
			if (bottomLeft.active)
			{
				configuration += 1;
			}
		}

	}

	// Nodes sit halfway between ControlNodes and 
	public class Node {
		public Vector3 position;
		public int vertexIndex = -1;

		public Node (Vector3 _pos) {
			position = _pos;
		}
	}

	// Control nodes represent the individual elements in MapGenerator.map
	// 0 is not active (free space in MapGenerator.map)
	// 1 is active (wall in MapGenerator.map)
	// Each ControlNode owns 2 Nodes
	// One above (halfway between it and its upper ControlNode neighbour)
	// One to the right (halfway between it and its neighbour to the right)
	public class ControlNode : Node {

		public bool active;
		public Node above, right;

		public ControlNode (Vector3 _pos, bool _active, float squareSize) : base(_pos) {
			active = _active;
			above = new Node (position + Vector3.forward * squareSize / 2f);
			right = new Node (position + Vector3.right * squareSize / 2f);
		}

	}
}
