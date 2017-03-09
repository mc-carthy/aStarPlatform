using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

	[SerializeField]
	private int width;
	[SerializeField]
	private int height;
	[SerializeField]
	private string seed;
	[SerializeField]
	private bool useRandomSeed;
	[SerializeField]
	[Range(0,100)]
	private int randomFillPercent;
	[SerializeField]
	private int numSmoothingIterations;

	private int [,] map;
	private int [,] borderedMap;
	private int borderSize = 5;

	private void Start() 
	{
		GenerateMap ();
	}

	private void Update() 
	{
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			GenerateMap ();
		}
	}

	// private void OnDrawGizmos ()
	// {
	// 	if (map != null)
	// 	{
	// 		for (int x = 0; x < width; x++)
	// 		{
	// 			for (int y = 0; y < height; y++)
	// 			{
	// 				Gizmos.color = (map [x, y] == 0) ? Color.white : Color.black;
	// 				Vector2 pos = new Vector2 (-width / 2f + x + 0.5f, -height / 2f + y + 0.5f);
	// 				Gizmos.DrawCube (pos, Vector3.one);
	// 			}
	// 		}
	// 	}
	// }

	private void GenerateMap() 
	{
		map = new int [width,height];

		RandomFillMap ();

		for (int i = 0; i < numSmoothingIterations; i ++) 
		{
			AlternativeSmoothMap ();
		}

		ProcessMap ();

		AddBorderToMap ();

		MeshGenerator meshGen = GetComponent<MeshGenerator> ();
		meshGen.GenerateMesh (borderedMap, 1);
	}

	private void AddBorderToMap ()
	{
		borderedMap = new int [width + borderSize * 2, height + borderSize * 2];

		for (int x = 0; x < borderedMap.GetLength (0); x ++) 
		{
			for (int y = 0; y < borderedMap.GetLength (1); y ++) 
			{
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) 
				{
					// Copy map to borderedMap
					borderedMap [x,y] = map [x - borderSize, y - borderSize];
				}
				else 
				{
					// Create border wall
					borderedMap [x,y] = 1;
				}
			}
		}
	}

	private void ProcessMap() 
	{
		// Remove all wall regions with a tile count of less than wallThresholdSize
		List<List<Coord>> wallRegions = GetRegions (1);
		int wallThresholdSize = 50;

		foreach (List<Coord> wallRegion in wallRegions) 
		{
			if (wallRegion.Count < wallThresholdSize) 
			{
				foreach (Coord tile in wallRegion) 
				{
					map [tile.tileX, tile.tileY] = 0;
				}
			}
		}

		// Remove all free space regions with a tile count of less than roomThresholdSize
		List<List<Coord>> roomRegions = GetRegions (0);
		int roomThresholdSize = 50;
		// Rooms larger than roomThresholdSize
		List<Room> survivingRooms = new List<Room> ();
		
		foreach (List<Coord> roomRegion in roomRegions) 
		{
			if (roomRegion.Count < roomThresholdSize) 
			{
				foreach (Coord tile in roomRegion) 
				{
					map [tile.tileX, tile.tileY] = 1;
				}
			}
			else 
			{
				survivingRooms.Add (new Room(roomRegion, map));
			}
		}

		// Order the rooms by size (largest first), set the largest as the main room, then ensure all other rooms are connected to it
		survivingRooms.Sort ();
		survivingRooms [0].isMainRoom = true;
		survivingRooms [0].isAccessibleFromMainRoom = true;

		ConnectClosestRooms (survivingRooms);
	}


	private void ConnectClosestRooms (List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) 
	{

		// All rooms, or !isAccessibleFromMainRoom rooms depending on forceAccessibilityFromMainRoom
		List<Room> roomListA = new List<Room> ();
		// All rooms, or isAccessibleFromMainRoom rooms depending on forceAccessibilityFromMainRoom
		List<Room> roomListB = new List<Room> ();

		if (forceAccessibilityFromMainRoom) 
		{
			foreach (Room room in allRooms) 
			{
				if (room.isAccessibleFromMainRoom) 
				{
					roomListB.Add (room);
				}
				else 
				{
					roomListA.Add (room);
				}
			}
		}
		else
		{
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;

		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		Room bestRoomA = new Room ();
		Room bestRoomB = new Room ();

		bool possibleConnectionFound = false;

		foreach (Room roomA in roomListA)
		{
			if (!forceAccessibilityFromMainRoom)
			{
				possibleConnectionFound = false;
				if (roomA.connectedRooms.Count > 0)
				{
					continue;
				}
			}

			foreach (Room roomB in roomListB)
			{
				if (roomA == roomB || roomA.IsConnected (roomB))
				{
					continue;
				}
			
				// This nested loop runs through the edge tiles of roomA and roomB and returns the points forming the closest line between the two rooms
				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++)
				{
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++)
					{
						Coord tileA = roomA.edgeTiles [tileIndexA];
						Coord tileB = roomB.edgeTiles [tileIndexB];

						// Ignore square root operation as it's expensive and unnecessary here
						int distanceBetweenRooms = (int) (Mathf.Pow (tileA.tileX-tileB.tileX, 2) + Mathf.Pow (tileA.tileY-tileB.tileY, 2));

						// If this point pair is the better than the previous pair or it's the first attempt
						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
						{
							bestDistance = distanceBetweenRooms;

							possibleConnectionFound = true;

							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}
			// If forceAccessibilityFromMainRoom we want to consider all connections to find the shortest, hence not creating a passage yet
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
			{
				CreatePassage (bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}

		// If forceAccessibilityFromMainRoom, we wait until outside of the loop in order to CreatePassage to ensure we create the shortest possible
		if (possibleConnectionFound && forceAccessibilityFromMainRoom)
		{
			CreatePassage (bestRoomA, bestRoomB, bestTileA, bestTileB);
			// Ensure complete connectivity
			ConnectClosestRooms (allRooms, true);
		}

		if (!forceAccessibilityFromMainRoom)
		{
			ConnectClosestRooms (allRooms, true);
		}
	}

	private void CreatePassage (Room roomA, Room roomB, Coord tileA, Coord tileB)
	{
		Room.ConnectRooms (roomA, roomB);
		//Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100f);

		List<Coord> line = GetLine (tileA, tileB);
		foreach (Coord c in line) {
			DrawCircle (c, 5);
		}
	}

	// Draw a "circle" radius r, of empty tiles around Coord c
	private void DrawCircle (Coord c, int r)
	{
		for (int x = -r; x <= r; x++)
		{
			for (int y = -r; y <= r; y++)
			{
				if ((x * x) + (y * y) <= (r * r))
				{
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if (IsInMapRange (drawX, drawY))
					{
						map [drawX, drawY] = 0;
					}
				}
			}
		}
	}

	// This returns a list of Coords that lie below the line drawn between two Coords
	private List<Coord> GetLine (Coord from, Coord to)
	{
		List<Coord> line = new List<Coord> ();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		// We assume dx is greater than dy, if dy > dx, we will set inverted to true
		// Greater in this instance means larger magnitude, i.e. dy: -4 is greater than dx: 2
		bool inverted = false;
		// The step is the direction in which x will be moving from this.from to this.to (or y, if inverted is true)
		int step = Math.Sign (dx);
		// The graident step is the direction in which y will be moving from this.from to this.to (or x, if inverted is true)
		int gradientStep = Math.Sign (dy);

		int longest = Mathf.Abs (dx);
		int shortest = Mathf.Abs (dy);

		if (longest < shortest)
		{
			inverted = true;
			longest = Mathf.Abs (dy);
			shortest = Mathf.Abs (dx);

			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}

		// This is the boundary at which we add a unit onto y (or x, if inverted)
		int gradientAccumulation = longest / 2;
		for (int i = 0; i < longest; i ++)
		{
			line.Add (new Coord (x, y));

			if (inverted)
			{
				y += step;
			}
			else
			{
				x += step;
			}

			gradientAccumulation += shortest;
			if (gradientAccumulation >= longest)
			{
				if (inverted)
				{
					x += gradientStep;
				}
				else
				{
					y += gradientStep;
				}
				// Revert gradientAccumulation
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	private Vector3 CoordToWorldPoint (Coord tile)
	{
		return new Vector3 (-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
	}

	// Return all regions with a specified tileType
	List<List<Coord>> GetRegions (int tileType)
	{
		List<List<Coord>> regions = new List<List<Coord>> ();
		int [,] mapFlags = new int [width, height];

		for (int x = 0; x < width; x ++)
		{
			for (int y = 0; y < height; y ++)
			{
				if (mapFlags [x, y] == 0 && map [x, y] == tileType)
				{
					List<Coord> newRegion = GetRegionTiles (x, y);
					regions.Add (newRegion);

					foreach (Coord tile in newRegion)
					{
						mapFlags [tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	// Flood fill algorithm that returns a list of tiles connected to the passed tile that form a region
	private List<Coord> GetRegionTiles (int startX, int startY)
	{
		List<Coord> tiles = new List<Coord> ();
		// This is an array that marks checked tiles as 1, unchecked as 0
		int [,] mapFlags = new int [width, height];
		// 0 Denotes free space, 1 denotes wall
		int tileType = map [startX, startY];

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		while (queue.Count > 0)
		{
			Coord tile = queue.Dequeue ();
			tiles.Add (tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
			{
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
				{
					if (IsInMapRange (x, y) && (y == tile.tileY || x == tile.tileX))
					{
						if (mapFlags [x, y] == 0 && map [x, y] == tileType)
						{
							mapFlags [x, y] = 1;
							queue.Enqueue (new Coord (x, y));
						}
					}
				}
			}
		}
		return tiles;
	}

	private bool IsInMapRange (int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}


	private void RandomFillMap ()
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString ();
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());

		for (int x = 0; x < width; x ++)
		{
			for (int y = 0; y < height; y ++)
			{
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					map [x, y] = 1;
				}
				else
				{
					map [x, y] = (pseudoRandom.Next (0,100) < randomFillPercent) ? 1 : 0;
				}
			}
		}
	}

    private void SmoothMap ()
    {
        int [,] newMap = new int [width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount (x, y);

                if (neighbourWallTiles > 4)
                {
                    newMap [x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    newMap [x, y] = 0;
                }

                if (x < 2 || x > width - 2 || y < 2 || y > height - 2)
                {
                    newMap [x, y] = 1;
                }
            }
        }
        map = newMap;
    }

	// This version ignores changes in surrounding tiles during the current smoothing cycle
	// Tends to lead to smoother, more confined maps
    private void AlternativeSmoothMap ()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount (x, y);

                if (neighbourWallTiles > 4)
                {
                    map [x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map [x, y] = 0;
                }
            }
        }
    }

	private int GetSurroundingWallCount (int gridX, int gridY) {
		int wallCount = 0;

		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++)
			{
				if (IsInMapRange (neighbourX, neighbourY))
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += map [neighbourX, neighbourY];
					}
				}
				else
				{
					wallCount ++;
				}
			}
		}

		return wallCount;
	}

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord (int x, int y)
		{
			tileX = x;
			tileY = y;
		}
	}


	class Room : IComparable<Room> {
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;

		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;

		public Room () {
		}

		public Room (List<Coord> roomTiles, int [,] map)
		{
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room> ();

			edgeTiles = new List<Coord> ();
			foreach (Coord tile in tiles)
			{
				for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
				{
					for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
					{
						if (x == tile.tileX || y == tile.tileY)
						{
							if (map [x, y] == 1)
							{
								edgeTiles.Add (tile);
							}
						}
					}
				}
			}
		}

		// Set a room and all of its connectedRooms as accessibleFromMainRoom
		public void SetAccessibleFromMainRoom () {
			if (!isAccessibleFromMainRoom)
			{
				isAccessibleFromMainRoom = true;
				foreach (Room connectedRoom in connectedRooms)
				{
					connectedRoom.SetAccessibleFromMainRoom ();
				}
			}
		}

		// Ensure roomA and roomB are added to each others connectedRooms List
		// Add all of roomA's other connectedRooms are added to roomB connectedRooms List and vice versa
		public static void ConnectRooms (Room roomA, Room roomB)
		{
			if (roomA.isAccessibleFromMainRoom)
			{
				roomB.SetAccessibleFromMainRoom ();
			}
			else if (roomB.isAccessibleFromMainRoom)
			{
				roomA.SetAccessibleFromMainRoom ();
			}
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		public bool IsConnected (Room otherRoom)
		{
			return connectedRooms.Contains (otherRoom);
		}

		// Returns -1 if smaller, 0 if the same, 1 if larger
		public int CompareTo (Room otherRoom)
		{
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}

}
