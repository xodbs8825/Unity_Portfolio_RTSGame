using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public float translationSpeed = 60f;
    public float altitude = 40f;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private Vector3 _forwardDir;

    private int _mouseOnScreenBorder;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        _mouseOnScreenBorder = -1;
    }

    private void Update()
    {
        if (_mouseOnScreenBorder >= 0)
        {
            TranslateCamera(_mouseOnScreenBorder);
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
                TranslateCamera(0);
            if (Input.GetKey(KeyCode.RightArrow))
                TranslateCamera(1);
            if (Input.GetKey(KeyCode.DownArrow))
                TranslateCamera(2);
            if (Input.GetKey(KeyCode.LeftArrow))
                TranslateCamera(3);
        }
    }

    private void TranslateCamera(int dir)
    {
        if (dir == 0) // 위
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed);
        else if (dir == 1) // 오른쪽
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2) // 아래
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed);
        else if (dir == 3) // 왼쪽
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

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
}
