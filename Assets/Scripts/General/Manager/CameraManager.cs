using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public float translationSpeed = 160f;
    private float _altitude = 60f;
    private float _zoomSpeed = 1000f;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private Vector3 _spawnPoint0 = new Vector3(115f, 60f, 15f);
    private Vector3 _spawnPoint1 = new Vector3(885f, 60f, 775f);

    private Vector3 _forwardDir;

    public Material miniMapIndicatorMaterial;
    private float _miniMapIndicatorStrokeWidth = 0.1f;
    private Transform _miniMapIndicator;
    private Mesh _miniMapIndicatorMesh;

    private int _mouseOnScreenBorder;
    private KeyCode[] _cameraTranslationKeyCode = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow
    };

    private float _maxZoomSize = 60f;

    public Transform groundTarget;
    public bool autoAdaptAltitude;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _mouseOnScreenBorder = -1;

        PrepareMapIndicator();

        groundTarget.position = Utils.MiddleOfScreenPointToWorld();
        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    }

    private void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (_mouseOnScreenBorder >= 0)
            TranslateCamera(_mouseOnScreenBorder);
        else
            for (int i = 0; i < _cameraTranslationKeyCode.Length; i++)
                if (Input.GetKey(_cameraTranslationKeyCode[i]))
                    TranslateCamera(i);

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0f)
            Zoom(Input.mouseScrollDelta.y > 0f ? -1 : 1);

        transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x, _spawnPoint0.x, _spawnPoint1.x),
            60f, Mathf.Clamp(Camera.main.transform.position.z, _spawnPoint0.z, _spawnPoint1.z));

        _miniMapIndicator.position = new Vector3(Mathf.Clamp(_miniMapIndicator.position.x, 115f, 885f), 0.1f,
            Mathf.Clamp(_miniMapIndicator.position.z, 118f, 887f));
    }

    public void SetPosition(int playerID)
    {
        transform.position = playerID == 0 ? _spawnPoint0 : _spawnPoint1;
    }

    private void TranslateCamera(int dir)
    {
        if (dir == 0)
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 1)
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2)     
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 3)                    
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

        if (autoAdaptAltitude)
            FixAltitude();

        ComputeMiniMapIndicator(false);
    }

    private void FixAltitude()
    {
        _ray = new Ray(transform.position, Vector3.up * -1000f);

        if (Physics.Raycast(_ray, out _hit, 1000f, Globals.TERRAIN_LAYER_MASK))
            transform.position = _hit.point + Vector3.up * _altitude;
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
        _camera.orthographicSize += zoomDir * Time.deltaTime * _zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, _maxZoomSize);

        ComputeMiniMapIndicator(true);
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
        groundTarget.position = middle;

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
}
