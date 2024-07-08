using UnityEngine;

namespace Game.Utils
{
    public static class Vector3Extensions
    {
        public static bool CheckDistanceTo(this Vector3 positionFrom, Vector3 positionTo, float distance)
        {
            return (positionTo - positionFrom).sqrMagnitude <= distance * distance;
        }
    }
}