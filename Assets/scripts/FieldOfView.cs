using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour {

    public float viewRadius;
    [RangeAttribute (0, 360)]
    public float viewAngle;
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform> ();

    [SerializeField]
    private LayerMask targetMask;
    [SerializeField]
    private LayerMask obstacleMask;
    [SerializeField]
    private float meshResolution;
    [SerializeField]
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    private int edgeResolveIterations = 5;
    private float edgeDistThreshold = 0.5f;
    private float maskCutawayDist = 1f;

    private void Start ()
    {
        StartCoroutine ("FindTargetsWithDelay", 0.2f);
        viewMesh = new Mesh ();
        viewMesh.name = "viewMesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void LateUpdate ()
    {
        DrawFieldOfView ();
    }

    public Vector3 DirFromAngle (float angleInDegrees, bool isGlobalAngle)
    {
        if (!isGlobalAngle)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3 (Mathf.Sin (angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos (angleInDegrees * Mathf.Deg2Rad));
    }

    private IEnumerator FindTargetsWithDelay (float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds (delay);
            FindVisibleTargets ();
        }
    }

    private void FindVisibleTargets ()
    {
        visibleTargets.Clear ();
        Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius [i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance (transform.position, target.position);
                if (!Physics.Raycast (transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add (target);
                }
            }
        }
    }

    private void DrawFieldOfView ()
    {
        int stepCount = Mathf.RoundToInt (viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3> ();
        ViewCastInfo oldViewCast = new ViewCastInfo ();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2f + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast (angle);

            if (i > 0)
            {
                bool edgeDistThresholdExceeded = Mathf.Abs (oldViewCast.dist - newViewCast.dist) > edgeDistThreshold;
                if ((oldViewCast.hit != newViewCast.hit) || (oldViewCast.hit && newViewCast.hit && edgeDistThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add (edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add (edge.pointB);
                    }
                }
            }
            viewPoints.Add (newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3 [vertexCount];
        int[] triangles = new int [(vertexCount - 2) * 3];

        vertices [0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices [i + 1] = transform.InverseTransformPoint (viewPoints [i] + transform.forward * maskCutawayDist);
            if (i < vertexCount - 2)
            {
                triangles [i * 3] = 0;
                triangles [i * 3 + 1] = i + 1;
                triangles [i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear ();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals ();
    }

    private ViewCastInfo ViewCast (float globalAngle)
    {
        Vector3 dir = DirFromAngle (globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    private EdgeInfo FindEdge (ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2f;
            ViewCastInfo newViewCast = ViewCast (angle);
            bool edgeDistThresholdExceeded = Mathf.Abs (minViewCast.dist - newViewCast.dist) > edgeDistThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo (minPoint, maxPoint);
    }

    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;

        public ViewCastInfo (bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct EdgeInfo {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo (Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}
