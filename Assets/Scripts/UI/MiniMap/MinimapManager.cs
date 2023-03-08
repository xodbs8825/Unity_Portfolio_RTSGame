using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapManager : MonoBehaviour
{
    private static Material _indicatorMat;

    public float lineWidth;

    private Camera _minimapCam;

    private void Start()
    {
        if (_indicatorMat == null)
            _indicatorMat = new Material(Shader.Find("Sprites/Default"));
        _minimapCam = GetComponent<Camera>();
    }

    public void OnPostRender()
    {
        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        Vector3 minViewportPoint = _minimapCam.WorldToViewportPoint(minWorldPoint);
        Vector3 maxViewportPoint = _minimapCam.WorldToViewportPoint(maxWorldPoint);

        float minX = minViewportPoint.x;
        float minY = minViewportPoint.y;
        float maxX = maxViewportPoint.x;
        float maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            _indicatorMat.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(1f, 1f, 0.85f));
            {
                GL.Vertex(new Vector3(minX, minY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + lineWidth, 0));

                GL.Vertex(new Vector3(minX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + lineWidth, maxY, 0));

                GL.Vertex(new Vector3(minX, maxY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + lineWidth, 0));

                GL.Vertex(new Vector3(maxX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + lineWidth, maxY, 0));
            }
            GL.End();
        }
        GL.PopMatrix();
    }

    //private void PrepareMapIndicator()
    //{
    //    GameObject g = new GameObject("MiniMapIndicator");
    //    _miniMapIndicator = g.transform;
    //    g.layer = 11; // put on "Minimap" layer
    //    _miniMapIndicator.position = Vector3.zero;
    //    _miniMapIndicatorMesh = CreateMiniMapIndicatorMesh();

    //    MeshFilter mf = g.AddComponent<MeshFilter>();
    //    mf.mesh = _miniMapIndicatorMesh;

    //    MeshRenderer mr = g.AddComponent<MeshRenderer>();
    //    mr.material = new Material(miniMapIndicatorMaterial);

    //    ComputeMiniMapIndicator(true);
    //}

    //private Mesh CreateMiniMapIndicatorMesh()
    //{
    //    Mesh m = new Mesh();

    //    Vector3[] vertices = new Vector3[] {
    //        Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,
    //        Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero
    //    };

    //    int[] triangles = new int[] {
    //        0, 4, 1, 4, 5, 1,
    //        0, 2, 6, 6, 4, 0,
    //        6, 2, 7, 2, 3, 7,
    //        5, 7, 3, 3, 1, 5
    //    };

    //    m.vertices = vertices;
    //    m.triangles = triangles;

    //    return m;
    //}

    //private void ComputeMiniMapIndicator(bool zooming)
    //{
    //    Vector3 middle = Utils.MiddleOfScreenPointToWorld();
    //    groundTarget.position = middle;

    //    // if zooming: recompute the indicator mesh
    //    if (zooming)
    //    {
    //        Vector3[] viewCorners = Utils.ScreenCornersToWorldPoints();

    //        float width = viewCorners[1].x - viewCorners[0].x;
    //        float height = viewCorners[2].z - viewCorners[0].z;

    //        for (int i = 0; i < 4; i++)
    //        {
    //            viewCorners[i].x -= middle.x;
    //            viewCorners[i].z -= middle.z;
    //        }

    //        Vector3[] innerCorners = new Vector3[]
    //        {
    //            new Vector3(viewCorners[0].x + _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[0].z + _miniMapIndicatorStrokeWidth * height),
    //            new Vector3(viewCorners[1].x - _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[1].z + _miniMapIndicatorStrokeWidth * height),
    //            new Vector3(viewCorners[2].x + _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[2].z - _miniMapIndicatorStrokeWidth * height),
    //            new Vector3(viewCorners[3].x - _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[3].z - _miniMapIndicatorStrokeWidth * height)
    //        };

    //        Vector3[] allCorners = new Vector3[]
    //        {
    //            viewCorners[0], viewCorners[1], viewCorners[2], viewCorners[3],
    //            innerCorners[0], innerCorners[1], innerCorners[2], innerCorners[3]
    //        };

    //        for (int i = 0; i < 8; i++)
    //            allCorners[i].y = 100f;

    //        _miniMapIndicatorMesh.vertices = allCorners;
    //        _miniMapIndicatorMesh.RecalculateNormals();
    //        _miniMapIndicatorMesh.RecalculateBounds();
    //    }

    //    // move the game object at the center of the main camera screen
    //    _miniMapIndicator.position = middle;
    //}
}
