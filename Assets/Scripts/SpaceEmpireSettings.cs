using System;
using UniKit;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    [Serializable]
    public struct SpaceEmpiresSettings
    {
        [SerializeField]
        private string name;
    }
    public class SpaceEmpireProjectSettings : GenericProjectSettings<SpaceEmpiresSettings, SpaceEmpireProjectSettings>
    {

    }
}
