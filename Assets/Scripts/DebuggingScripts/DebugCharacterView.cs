using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DebugCharacterView : MonoBehaviour
{
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    [SerializeField] private int viewMeshResolution = 20; // Number of rays cast for view mesh
    [SerializeField] private Material viewMeshMaterial; // Material for the view mesh
    public float drawAngle = 100;
    public LayerMask obstacleLayers;

    // Start is called before the first frame update
    void Start()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    // Update is called once per frame
    void Update()
    {
        DrawViewMesh();
    }

    private void DrawViewMesh()
    {
        int stepCount = Mathf.RoundToInt(drawAngle * viewMeshResolution / 10f);
        float stepAngleSize = drawAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - drawAngle / 2 + stepAngleSize * i;
            ViewCastInfo viewCast = ViewCast(angle);
            viewPoints.Add(viewCast.point);
        }

        int vertexCount = viewPoints.Count + 1; // +1 for origin
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // Origin point at the enemy's position

        for (int i = 0; i < viewPoints.Count; i++)
        {
            // Convert world point to local
            Vector3 localPoint = transform.InverseTransformPoint(viewPoints[i]);
            vertices[i + 1] = localPoint;

            if (i < viewPoints.Count - 1)
            {
                triangles[i * 3] = 0; // Origin
                triangles[i * 3 + 1] = i + 1; // Current point
                triangles[i * 3 + 2] = i + 2; // Next point
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, drawAngle, obstacleLayers))
        {
            return new ViewCastInfo(hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(transform.position + dir * drawAngle, drawAngle, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

        struct ViewCastInfo
    {
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(Vector3 _point, float _distance, float _angle)
        {
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }
}
