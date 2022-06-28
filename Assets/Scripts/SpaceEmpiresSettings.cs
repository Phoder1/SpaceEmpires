using UniKit;
using UnityEngine;

namespace Phoder1.SpaceEmpires
{
    public class SpaceEmpiresSettings : GenericProjectSettings<SpaceEmpiresSettings>
    {
        [SerializeField]
        private float tileSize = 1;
        public static float TileSize => Data.tileSize;
        [SerializeField]
        private int initialResources = 50;
        public static int InitialResources => Data.initialResources;
        [SerializeField]
        private int killReward = 20;
        public static int KillReward => Data.killReward;
        [SerializeField]
        private GameObject soldierPrefab;
        public static GameObject SoldierPrefab => Data.soldierPrefab;
        [SerializeField]
        private GameObject workerPrefab;
        public static GameObject WorkerPrefab => Data.workerPrefab;

    }
}
