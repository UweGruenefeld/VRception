using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Extension method for the Vector2 class that allows to rotate vectors
    /// </summary>
    public static class Vector2Extension
    {
        // Method allows to rotate a Vector2 by the specfied degress 
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            return new Vector2(
                cos * v.x - sin * v.y, 
                sin * v.x + cos * v.y
            );
        }
    }
}