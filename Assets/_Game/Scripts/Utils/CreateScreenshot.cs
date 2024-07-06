using UnityEngine;

namespace Game.Utils
{
    [RequireComponent(typeof(Camera))]
    public class CreateScreenshot : MonoBehaviour
    {
        [ContextMenu("Take Screenshot")]
        private void TakeScreenshot()
        {
            var width = 7680;
            var height = 2160;
            var camera = GetComponent<Camera>();
            var rt = new RenderTexture(width, height, 24);
            camera.targetTexture = rt;
            var screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            var bytes = screenShot.EncodeToPNG();
            DestroyImmediate(screenShot);
            System.IO.File.WriteAllBytes($"{Application.dataPath}/screenshot.png", bytes);
        }
    }
}