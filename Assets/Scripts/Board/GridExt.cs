using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public static class GridExt
    {
        public static Vector2 RoundToGrid(this Vector2 vector, float gridSize)
        {
            var absX = Mathf.Abs(vector.x);
            vector.x = Mathf.Sign(vector.x) * Mathf.Round(absX / gridSize) * gridSize;

            var absY = Mathf.Abs(vector.y);
            vector.y = Mathf.Sign(vector.y) * Mathf.Round(absY / gridSize) * gridSize;

            return vector;
        }
    }
}
