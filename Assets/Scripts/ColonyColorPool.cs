using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public class ColonyColorPool : MonoBehaviour
    {
        [SerializeField]
        private Color startColor;
        [SerializeField]
        private bool troll = false;
        [SerializeField]
        private float trollCycleTime = 10;

        Dictionary<IColony, ReactiveProperty<Color>> colonyColors = new Dictionary<IColony, ReactiveProperty<Color>>();
        Dictionary<IColony, IReadOnlyReactiveProperty<Color>> readonlyColonyColors = new Dictionary<IColony, IReadOnlyReactiveProperty<Color>>();
        public IReadOnlyDictionary<IColony, IReadOnlyReactiveProperty<Color>> ColonyColors => readonlyColonyColors;

        private void Update()
        {
            if (troll)
            {
                foreach (KeyValuePair<IColony, ReactiveProperty<Color>> color in colonyColors)
                {
                    Color.RGBToHSV(color.Value.Value, out var h, out var s, out var v);
                    var hStep = Time.deltaTime / trollCycleTime;
                    color.Value.Value = Color.HSVToRGB(Normalized(h + hStep), s, v);
                }
            }
        }
        public IReadOnlyReactiveProperty<Color> AddColony(IColony newColony)
        {
            if (colonyColors.TryGetValue(newColony, out var newColonyColor))
                return newColonyColor;

            var property = new ReactiveProperty<Color>();
            colonyColors.Add(newColony, property);
            readonlyColonyColors.Add(newColony, property);

            UpdateColors();

            return property;
        }

        private void UpdateColors()
        {
            var colors = GenerateColors(colonyColors.Count);

            var colonies = colonyColors.Keys.ToArray();
            for (int i = 0; i < colonies.Length; i++)
                colonyColors[colonies[i]].Value = colors[i];
        }
        private Color[] GenerateColors(int amount)
        {
            Color.RGBToHSV(startColor, out var h, out var s, out var v);
            var hueStep = 1f / (float)amount;

            Color[] colors = new Color[amount];
            for (int i = 0; i < amount; i++)
                colors[i] = Color.HSVToRGB(Normalized(h + hueStep * i), s, v);

            return colors;
        }

        private float Normalized(float value)
            => Mathf.Abs(value) % 1f;

        private void OnValidate()
        {
            if (Application.isPlaying)
                UpdateColors();
        }
    }
}
