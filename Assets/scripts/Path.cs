using UnityEngine;

public class Path {

	public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;

    public Path (Vector3[] waypoints, Vector3 startPos, float turnDistance)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = Vector3ToVector2 (startPos);
        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = Vector3ToVector2 (lookPoints [i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDistance;
            turnBoundaries [i] = new Line (turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDistance);
            previousPoint = turnBoundaryPoint;
        }
    }

    private Vector2 Vector3ToVector2 (Vector3 vec3)
    {
        return new Vector2 (vec3.x, vec3.z);
    }

    public void DrawWithGizmos ()
    {
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawCube (p + Vector3.up, Vector3.one);
        }
        Gizmos.color = Color.white;
        foreach (Line l in turnBoundaries)
        {
            l.DrawWithGizmos (10);
        }
    }

}
