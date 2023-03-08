using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogRendererToggler : MonoBehaviour
{
    public Renderer myRenderer;
    [Range(0f, 1f)] public float threshold = 0.1f;

    private Camera _camera;

    private static Texture2D _shadowTexture;
    private static Rect _rect;
    private static bool _isDirty = true;

    private void Awake()
    {
        _camera = GameObject.Find("FogOfWar/UnexploredAreasCam").GetComponent<Camera>();
    }

    private Color GetColorAtPosition()
    {
        if (!_camera)
            return Color.white;

        RenderTexture renderTexture = _camera.targetTexture;
        if (!renderTexture)
            return _camera.backgroundColor;

        if (_shadowTexture == null || renderTexture.width != _rect.width || renderTexture.height != _rect.height)
        {
            _rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            _shadowTexture = new Texture2D((int)_rect.width, (int)_rect.height, TextureFormat.RGB24, false);
        }

        if (_isDirty)
        {
            RenderTexture.active = renderTexture;
            _shadowTexture.ReadPixels(_rect, 0, 0);
            RenderTexture.active = null;
            _isDirty = false;
        }

        var pixel = _camera.WorldToScreenPoint(transform.position);
        return _shadowTexture.GetPixel((int)pixel.x, (int)pixel.y);
    }

    private void Update()
    {
        _isDirty = true;
    }

    private void LateUpdate()
    {
        if (!myRenderer)
        {
            this.enabled = false;
            return;
        }

        myRenderer.enabled = GetColorAtPosition().grayscale >= threshold;
    }
}
