using UnityEngine;

public class BspGridLeaf {

    private const int MIN_LEAF_SIZE = 12;

    public BspGridLeaf parent;
    public BspGridLeaf firstChild;
    public BspGridLeaf secondChild;

    public int width;
    public int height;
    public int leftX;
    public int bottomY;
    public int roomMinDimension = 4;
    public int roomLeafBoundary = 4;
    public bool hasRoom;
    public Vector2 roomSize;
    public Vector2 roomPos;

    public Room room;

    public bool isFirstChild;
    public Vector2 centre;

    private GameObject quad;

    public BspGridLeaf (int _leftX, int _bottomY, int _width, int _height, BspGridLeaf _parent)
    {
        leftX = _leftX;
        bottomY = _bottomY;
        width = _width;
        height = _height;
        parent = _parent;

        hasRoom = false;
        centre = new Vector2 (leftX + width / 2, bottomY + height / 2);
    }

    public bool Split ()
    {
        // If this leaf already has children, skip it
        if (firstChild != null || secondChild != null)
        {
            return false;
        }

        // Small chance of leaf not splitting leading to a large room
        if (Random.Range (0f, 1f) < 0.2f && parent != null)
        {
            return false;
        }

        // 50:50 chance to split horizontally or vertically
        bool splitH = Random.Range (0f, 1f) < 0.5f;

        // If the width is greater than 1.25 height, split vertically
        if (width / height > 1.25f)
        {
            splitH = false;
        }
        // If the height is greater than 1.25 width, split horizontally
        if (height / width > 1.25f)
        {
            splitH = true;
        }

        // Determine the max height or width (dependant on splitH) of the child leaf
        int max = (splitH ? height : width) - MIN_LEAF_SIZE;

        // If the max is less than minimum size allowed, return false
        if (max <= MIN_LEAF_SIZE)
        {
            return false;
        }

        // Generate split
        int split = Random.Range (MIN_LEAF_SIZE, max);

        if (splitH)
        {
            firstChild = new BspGridLeaf (leftX, bottomY, width, split, this);
            secondChild = new BspGridLeaf (leftX, bottomY + split, width, height - split, this);
        }
        else
        {
            firstChild = new BspGridLeaf (leftX, bottomY, split, height, this);
            secondChild = new BspGridLeaf (leftX + split, bottomY, width - split, height, this);
        }

        // If we've got this far, we've successfully split the leaf
        return true;
    }

    public void CreateRooms ()
    {
        if (firstChild != null || secondChild != null)
        {
            hasRoom = false;
            if (firstChild != null)
            {
                firstChild.CreateRooms ();
            }
            if (secondChild != null)
            {
                secondChild.CreateRooms ();
            }
        }
        else
        {
            roomSize = new Vector2 (
                Random.Range (roomMinDimension, width - roomLeafBoundary), 
                Random.Range (roomMinDimension, height - roomLeafBoundary)
            );
            roomPos = new Vector2 (
                Random.Range (roomLeafBoundary, width - (int)roomSize.x - roomLeafBoundary),
                Random.Range (roomLeafBoundary, height - (int)roomSize.y - roomLeafBoundary)
            );
            
            // Debug.Log ("Min x = " + (roomLeafBoundary).ToString ());
            // Debug.Log (" - Max x = " + (width - (int)roomSize.x - roomLeafBoundary).ToString ());
            // Debug.Log ("Min y = " + (roomLeafBoundary).ToString ());
            // Debug.Log (" - Max y = " + (height - (int)roomSize.y - roomLeafBoundary).ToString ());

            room = new Room (roomSize, roomPos + new Vector2 (leftX, bottomY), this);  

            hasRoom = true;
        }
    }

    public class Room {

        public Vector2 size;
        public BspGridLeaf parent;
        public Vector2 bottomLeft;
        public Vector2 centrePos;

        public Room (Vector2 _size, Vector2 _bottomLeft, BspGridLeaf _parent)
        {
            size = _size;
            bottomLeft = _bottomLeft;
            parent = _parent;

            centrePos = bottomLeft + size / 2;
        }
    }

}