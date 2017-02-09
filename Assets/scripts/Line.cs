using UnityEngine;

public struct Line {

    const float verticalLineGradient = 1e5f;

	private float gradient;
    private float y_intercept;
    private float gradientPerpendicular;
    private Vector2 pointOnLine_1;
    private Vector2 pointOnLine_2;
    private bool approachSide;

    public Line (Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if (dx != 0)
        {
            gradientPerpendicular = dy / dx;
        }
        else {
            gradientPerpendicular = verticalLineGradient;
        }

        if (gradientPerpendicular != 0)
        {
            gradient = -1 / gradientPerpendicular;
        }
        else
        {
            gradient = verticalLineGradient;
        }

        y_intercept = pointOnLine.y - gradient * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2 (1, gradient);

        approachSide = false;
        approachSide = GetSide (pointPerpendicularToLine);
    }

    private bool GetSide (Vector2 point)
    {
        return (point.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (point.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    public bool HasCrossedLine (Vector2 point)
    {
        return GetSide (point) != approachSide;
    }

    public void DrawWithGizmos (float length)
    {
        Vector3 lineDir = new Vector3 (1, 0, gradient).normalized;
        Vector3 lineCenter = new Vector3 (pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine (lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);
    }

}
