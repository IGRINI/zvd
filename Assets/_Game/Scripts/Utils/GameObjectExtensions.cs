using UnityEngine;

namespace Game.Utils
{
    public static class GameObjectExtensions
    {
        public static void Destroy(this Object obj, bool deleteAsset = false)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Object.DestroyImmediate(obj, deleteAsset);
            }
            else
            {
                Object.Destroy(obj);
            }
        }
    }
}