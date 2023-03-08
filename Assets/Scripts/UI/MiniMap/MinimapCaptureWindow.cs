using UnityEngine;
using UnityEditor;

public class MinimapCaptureWindow : EditorWindow
{
    private Transform _screenshotCameraAnchor;
    private Camera _screenshotCamera;
    private string _screenshotName = "map";
    private float _mapSize = 1000;
    private float _imageSize = 1080;

    [MenuItem("Tools/Minimap Capture")]
    public static void Init()
    {
        // 프로젝트 상에 존재하는 열려있는 윈도우 가져오기 아니면 새로 만들기
        MinimapCaptureWindow window = (MinimapCaptureWindow)GetWindow(typeof(MinimapCaptureWindow));

        // 타이틀하고 아이콘 세팅
        GUIContent titleContent = new GUIContent("Minimap Capture");
        window.titleContent = titleContent;
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Minimap Capture", EditorStyles.boldLabel);

        _screenshotCameraAnchor = (Transform)EditorGUILayout.ObjectField("Camera Anchor", _screenshotCameraAnchor,
            typeof(Transform), true);
        _screenshotCamera = (Camera)EditorGUILayout.ObjectField("Camera", _screenshotCamera, typeof(Camera), true);
        _screenshotName = EditorGUILayout.TextField("Name", _screenshotName);
        _mapSize = EditorGUILayout.FloatField("Map Size", _mapSize);
        _imageSize = EditorGUILayout.FloatField("Image Size", _imageSize);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(_screenshotCameraAnchor == null || _screenshotCamera == null);
        if (GUILayout.Button("Take Screenshot"))
        {
            Vector3 prevAnchorPos = _screenshotCameraAnchor.position;
            float preOrthoSize = _screenshotCamera.orthographicSize;

            float t = _mapSize / 2;
            _screenshotCameraAnchor.position = new Vector3(t, 0, t);
            _screenshotCamera.orthographicSize = t;

            MinimapCapture.TakeScreenshot(_screenshotName, new Vector2Int((int)_imageSize, (int)_imageSize), _screenshotCamera);

            _screenshotCameraAnchor.position = prevAnchorPos;
            _screenshotCamera.orthographicSize = preOrthoSize;
        }
        EditorGUI.EndDisabledGroup();
    }
}
