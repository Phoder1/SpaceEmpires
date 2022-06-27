using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public class HpBarInstaller : MonoInstaller
    {
        [SerializeField]
        private Image hpBar;
        public override void InstallBindings()
        {
            Container.Bind<Image>().WithId("HpBar").FromInstance(hpBar);
        }
    }
}
