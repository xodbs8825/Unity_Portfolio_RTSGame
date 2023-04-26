using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Minimap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 terrainSize;
    private Vector2 _uiSize;

    private Vector2 _offset;
    private Vector2 _lastPointerPosition;
    private bool _dragging = false;

    public RectTransform minimapContainerRectTransform;

    void Start()
    {
        _offset = minimapContainerRectTransform.anchoredPosition;
        _uiSize = GetComponent<RectTransform>().sizeDelta;
        _lastPointerPosition = Input.mousePosition;
    }

    void Update()
    {
        if (!_dragging) return;

        Vector2 delta = (Vector2)Input.mousePosition - _lastPointerPosition;
        _lastPointerPosition = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0f, 300f), Mathf.Clamp(Input.mousePosition.y, 0f, 300f));

        if (delta.magnitude > Mathf.Epsilon)
        {
            Vector2 uiPos = (new Vector2(_lastPointerPosition.x, _lastPointerPosition.y) / GameManager.instance.canvasScaleFactor) - _offset;
            Vector3 realPos = new Vector3(uiPos.x / _uiSize.x * terrainSize.x, 0f, uiPos.y / _uiSize.y * terrainSize.y);
            realPos = Utils.ProjectOnTerrain(realPos);

            EventManager.TriggerEvent("ClickedMinimap", realPos);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
    }
}
