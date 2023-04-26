using System;
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

    private int _mouseOnScreenBorder;
    private KeyCode[] _cameraTranslationKeyCode = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow
    };

    private bool _placingBuilding;

    private float _distance = 500f;
    private float _maxZoomSize = 60f;

    private float _minX;
    private float _minZ;
    private float _maxX;
    private float _maxZ;
    private float _camMinimapBuffer = 5f;

    private Vector3 _camOffset;
    private Vector3 _camHalfViewZone;

    public Transform groundTarget;
    public bool autoAdaptAltitude;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _placingBuilding = false;
        _mouseOnScreenBorder = -1;

        InitializeBounds();
    }

    private void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (_mouseOnScreenBorder >= 0)
        {
            TranslateCamera(_mouseOnScreenBorder);
        }
        else
        {
            for (int i = 0; i < _cameraTranslationKeyCode.Length; i++)
            {
                if (Input.GetKey(_cameraTranslationKeyCode[i]))
                {
                    TranslateCamera(i);
                }
            }
        }

        if (!_placingBuilding && Math.Abs(Input.mouseScrollDelta.y) > 0f)
        {
            Zoom(Input.mouseScrollDelta.y > 0f ? 1 : -1);
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener("PlaceBuildingOn", OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", OnPlaceBuildingOff);
        EventManager.AddListener("ClickedMinimap", OnClickedMinimap);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaceBuildingOn", OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", OnPlaceBuildingOff);
        EventManager.RemoveListener("ClickedMinimap", OnClickedMinimap);
    }

    private void OnPlaceBuildingOn()
    {
        _placingBuilding = true;
    }

    private void OnPlaceBuildingOff()
    {
        _placingBuilding = false;
    }

    private void OnClickedMinimap(object data)
    {
        Vector3 pos = FixBounds((Vector3)data);
        SetPosition(pos);

        if (autoAdaptAltitude)
        {
            FixAltitude();
        }
    }

    public void OnMouseEnterScreenBorder(int borderIndex)
    {
        _mouseOnScreenBorder = borderIndex;
    }

    public void OnMouseExitScreenBorder()
    {
        _mouseOnScreenBorder = -1;
    }

    public void SetPosition(int playerID)
    {
        transform.position = playerID == 0 ? _spawnPoint0 : _spawnPoint1;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position - _distance * transform.forward;
        FixGroundTarget();
    }

    private void TranslateCamera(int dir)
    {
        if (dir == 0 && transform.position.z <= _maxZ)
        {
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        }
        else if (dir == 1 && transform.position.x + _camHalfViewZone.x <= _maxX)
        {
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        }
        else if (dir == 2 && transform.position.z >= _minZ)
        {
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        }
        else if (dir == 3 && transform.position.x - _camHalfViewZone.x >= _minX)
        {
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);
        }

        FixGroundTarget();

        if (autoAdaptAltitude)
            FixAltitude();
    }

    private void FixAltitude()
    {
        _ray = new Ray(transform.position, Vector3.up * -1000f);

        if (Physics.Raycast(_ray, out _hit, 1000f, Globals.TERRAIN_LAYER_MASK))
            transform.position = _hit.point + Vector3.up * _altitude;
    }

    private void FixGroundTarget()
    {
        groundTarget.position = Utils.MiddleOfScreenPointToWorld(_camera);
    }

    private void Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * _zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, _maxZoomSize);

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;

        Vector3 pos = Utils.MiddleOfScreenPointToWorld();
        pos = FixBounds(pos);
        SetPosition(pos);
    }

    private Vector3 FixBounds(Vector3 position)
    {
        if (position.x - _camHalfViewZone.x < _minX) position.x = _minX + _camHalfViewZone.x;
        if (position.x + _camHalfViewZone.x > _maxX) position.x = _maxX - _camHalfViewZone.x;
        if (position.z - _camHalfViewZone.z < _minZ) position.z = _minZ + _camHalfViewZone.z;
        if (position.z + _camHalfViewZone.z > _maxZ) position.z = _maxZ - _camHalfViewZone.z;

        return position;
    }

    public void InitializeBounds()
    {
        _minX = 0;
        _maxX = GameManager.instance.terrainSize;
        _minZ = -312f;
        _maxZ = 445.99f;

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;

        FixGroundTarget();
    }
}
