using System.Collections;
using System.Collections.Generic;
using UniKit;
using UnityEngine;
using Zenject;

namespace Phoder1.SpaceEmpires
{
    public class MainSceneSequence : MonoBehaviour
    {
        [Inject]
        private ITurns turns;

        private void Start()
        {
            turns.StartTurns();
        }
    }
}
