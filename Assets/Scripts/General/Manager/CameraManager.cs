using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    private float translationSpeed = 160f;
    private float altitude = 60f;
    private float zoomSpeed = 1000f;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private int _mouseOnScreenBorder;
    private KeyCode[] _cameraTranslationKeyCode = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow
    };

    public Material miniMapIndicatorMaterial;
    private float _miniMapIndicatorStrokeWidth = 0.1f;
    private Transform _miniMapIndicator;
    private Mesh _miniMapIndicatorMesh;

    private float _maxZoomSize = 60f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _mouseOnScreenBorder = -1;
        PrepareMapIndicator();
    }

    private void Update()
    {
        if (_mouseOnScreenBorder >= 0)
            TranslateCamera(_mouseOnScreenBorder);
        else
            for (int i = 0; i < _cameraTranslationKeyCode.Length; i++)
                if (Input.GetKey(_cameraTranslationKeyCode[i]))
                    TranslateCamera(i);

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0f)
            Zoom(Input.mouseScrollDelta.y > 0f ? -1 : 1);
    }

    private void TranslateCamera(int dir)
    {
        if (dir == 0) // 위
            transform.Translate(transform.forward * Time.deltaTime * translationSpeed * 2);
        else if (dir == 1) // 오른쪽
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2) // 아래
            transform.Translate(-transform.forward * Time.deltaTime * translationSpeed * 2);
        else if (dir == 3) // 왼쪽
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

        _ray = new Ray(transform.position, Vector3.up * -1000f);
        if (Physics.Raycast(_ray, out _hit, 1000f, Globals.TERRAIN_LAYER_MASK))
            transform.position = _hit.point + Vector3.up * altitude;

        FixAltitude();
        ComputeMiniMapIndicator(false);
    }

    private void FixAltitude()
    {
        _ray = new Ray(transform.position, Vector3.up * -1000f);

        if (Physics.Raycast(_ray, out _hit, 1000f, Globals.TERRAIN_LAYER_MASK))
            transform.position = _hit.point + Vector3.up * altitude;
    }

    public void OnMouseEnterScreenBorder(int borderIndex)
    {
        _mouseOnScreenBorder = borderIndex;
    }

    public void OnMouseExitScreenBorder()
    {
        _mouseOnScreenBorder = -1;
    }

    private void Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, _maxZoomSize);

        ComputeMiniMapIndicator(true);
    }

    private void PrepareMapIndicator()
    {
        GameObject g = new GameObject("MiniMapIndicator");
        _miniMapIndicator = g.transform;
        g.layer = 11; // put on "Minimap" layer
        _miniMapIndicator.position = Vector3.zero;
        _miniMapIndicatorMesh = CreateMiniMapIndicatorMesh();

        MeshFilter mf = g.AddComponent<MeshFilter>();
        mf.mesh = _miniMapIndicatorMesh;

        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        mr.material = new Material(miniMapIndicatorMaterial);

        ComputeMiniMapIndicator(true);
    }

    private Mesh CreateMiniMapIndicatorMesh()
    {
        Mesh m = new Mesh();
       
        Vector3[] vertices = new Vector3[] {
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero
        };
        
        int[] triangles = new int[] {
            0, 4, 1, 4, 5, 1,
            0, 2, 6, 6, 4, 0,
            6, 2, 7, 2, 3, 7,
            5, 7, 3, 3, 1, 5
        };
        
        m.vertices = vertices;
        m.triangles = triangles;
        
        return m;
    }

    private void ComputeMiniMapIndicator(bool zooming)
    {
        Vector3 middle = Utils.MiddleOfScreenPointToWorld();
        // if zooming: recompute the indicator mesh
        if (zooming)
        {
            Vector3[] viewCorners = Utils.ScreenCornersToWorldPoints();
            
            float width = viewCorners[1].x - viewCorners[0].x;
            float height = viewCorners[2].z - viewCorners[0].z;
            
            for (int i = 0; i < 4; i++)
            {
                viewCorners[i].x -= middle.x;
                viewCorners[i].z -= middle.z;
            }
            
            Vector3[] innerCorners = new Vector3[]
            {
                new Vector3(viewCorners[0].x + _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[0].z + _miniMapIndicatorStrokeWidth * height),
                new Vector3(viewCorners[1].x - _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[1].z + _miniMapIndicatorStrokeWidth * height),
                new Vector3(viewCorners[2].x + _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[2].z - _miniMapIndicatorStrokeWidth * height),
                new Vector3(viewCorners[3].x - _miniMapIndicatorStrokeWidth * width, 0f, viewCorners[3].z - _miniMapIndicatorStrokeWidth * height)
            };
            
            Vector3[] allCorners = new Vector3[]
            {
                viewCorners[0], viewCorners[1], viewCorners[2], viewCorners[3],
                innerCorners[0], innerCorners[1], innerCorners[2], innerCorners[3]
            };
            
            for (int i = 0; i < 8; i++)
                allCorners[i].y = 100f;
            
            _miniMapIndicatorMesh.vertices = allCorners;
            _miniMapIndicatorMesh.RecalculateNormals();
            _miniMapIndicatorMesh.RecalculateBounds();
        }
        
        // move the game object at the center of the main camera screen
        _miniMapIndicator.position = middle;
    }

    private void OnEnable()
    {
        EventManager.AddListener("MoveCamera", OnMoveCamera);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("MoveCamera", OnMoveCamera);
    }

    private void OnMoveCamera(object data)
    {
        Vector3 pos = (Vector3)data;
        
        float indicatorWidth = _miniMapIndicatorMesh.vertices[1].x - _miniMapIndicatorMesh.vertices[0].x;
        float indicatorHeight = _miniMapIndicatorMesh.vertices[2].z - _miniMapIndicatorMesh.vertices[0].z;

        pos.x -= indicatorWidth / 2f;
        pos.z -= indicatorHeight / 2f;

        Vector3 off = transform.position - Utils.MiddleOfScreenPointToWorld();
        Vector3 newPos = pos + off;

        newPos.y = 100f;
        transform.position = newPos;

        FixAltitude();
        ComputeMiniMapIndicator(false);
    }
}
