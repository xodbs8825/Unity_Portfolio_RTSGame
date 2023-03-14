using System.IO;
using UnityEngine;

public static class MinimapCapture
{
    public static void TakeScreenshot(string name, Vector2Int size, Camera camera)
    {
        RenderTexture prevRenderTexture = camera.targetTexture;

        // ���� �ؽ��� �غ�
        RenderTexture renderTexture = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 4;

        camera.targetTexture = renderTexture;
        camera.Render();

        // �ؽ��� 2D�� ����
        Texture2D output = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

        RenderTexture.active = renderTexture;
        output.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);

        byte[] bytes = output.EncodeToJPG(90);
        Object.DestroyImmediate(output);

        // ��ũ���� ���� ������ ���� ����
        string folderPath = Application.dataPath + "/Resources/MapCaptures";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = $"{folderPath}/{name}.jpg";
        File.WriteAllBytes(filePath, bytes);

        // �ʱ�ȭ
        RenderTexture.active = null;
        camera.targetTexture = prevRenderTexture;
        renderTexture.DiscardContents();

        Debug.Log($"Screenshot taken : {filePath}");
    }
}
