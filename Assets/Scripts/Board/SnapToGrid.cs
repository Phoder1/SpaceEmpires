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
        private float gridSize;

        private void Update()
        {
            if (Application.isPlaying)
                return;
            if (!ProjectPrefs.HasKey("Grid size"))
                return;

            UpdateGridSize();
            transform.position = ((Vector2)transform.position).RoundToGrid(gridSize);
            gridPosition = Vector2Int.RoundToInt(transform.position / gridSize);
        }
        private void UpdateGridSize()
        {
            var newGridSize = ProjectPrefs.GetFloat("Grid size");

            gridSize = newGridSize;
        }
#endif
    }
}
