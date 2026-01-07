using System;
using UnityEngine;
using VContainer;

namespace Architecture
{
    public class GamePlayExit : MonoBehaviour
    {
        [Inject] private GameFlowController _gameFlowController;

        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
        }

        public void OnExitClicked()
        {
            _gameFlowController.ExitGamePlay();
        }
    }
}