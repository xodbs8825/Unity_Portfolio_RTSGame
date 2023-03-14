using System.IO;
using UnityEngine;

public static class MinimapCapture
{
    public static void TakeScreenshot(string name, Vector2Int size, Camera camera)
    {
        RenderTexture prevRenderTexture = camera.targetTexture;

        // 랜더 텍스쳐 준비
        RenderTexture renderTexture = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 4;

        camera.targetTexture = renderTexture;
        camera.Render();

        // 텍스쳐 2D로 변경
        Texture2D output = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

        RenderTexture.active = renderTexture;
        output.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);

        byte[] bytes = output.EncodeToJPG(90);
        Object.DestroyImmediate(output);

        // 스크린샷 저장 폴더에 파일 저장
        string folderPath = Application.dataPath + "/Resources/MapCaptures";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = $"{folderPath}/{name}.jpg";
        File.WriteAllBytes(filePath, bytes);

        // 초기화
        RenderTexture.active = null;
        camera.targetTexture = prevRenderTexture;
        renderTexture.DiscardContents();

        Debug.Log($"Screenshot taken : {filePath}");
    }
}
