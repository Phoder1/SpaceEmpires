using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

namespace Phoder1.SpaceEmpires
{
    public class ColonyColorPool : MonoBehaviour
    {
        [SerializeField]
        private Color startColor;

        Dictionary<IColony, ReactiveProperty<Color>> colonyColors = new Dictionary<IColony, ReactiveProperty<Color>>();
        Dictionary<IColony, IReadOnlyReactiveProperty<Color>> readonlyColonyColors = new Dictionary<IColony, IReadOnlyReactiveProperty<Color>>();
        public IReadOnlyDictionary<IColony, IReadOnlyReactiveProperty<Color>> ColonyColors => readonlyColonyColors;

        public IReadOnlyReactiveProperty<Color> AddColony(IColony newColony)
        {
            if(colonyColors.TryGetValue(newColony, out var newColonyColor))
                return newColonyColor;

            var colors = GenerateColors(colonyColors.Count + 1);

            var colonies = colonyColors.Keys.ToArray();
            for (int i = 0; i < colonies.Length; i++)
                colonyColors[colonies[i]].Value = colors[i];

            var property = new ReactiveProperty<Color>(colors[colors.Length - 1]);
            colonyColors.Add(newColony, property);
            readonlyColonyColors.Add(newColony, property);

            return property;
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
    }
}
