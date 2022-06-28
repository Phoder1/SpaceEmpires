using UniKit.Project;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    [ExecuteInEditMode]
    public class SnapToGrid : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Vector2Int gridPosition;
#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying)
                return;
            var gridSize = SpaceEmpiresSettings.TileSize;
            if (Mathf.Approximately(gridSize, 0))
                return;

            transform.position = ((Vector2)transform.position).RoundToGrid(gridSize);
            gridPosition = Vector2Int.RoundToInt(transform.position / gridSize);
        }
#endif
    }
}
