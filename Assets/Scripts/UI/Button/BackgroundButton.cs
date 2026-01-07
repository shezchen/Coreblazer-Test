using System;
using Architecture;
using GamePlay;
using UnityEngine;
using VContainer;

namespace UI
{
    public class BackgroundButton : MonoBehaviour
    {
        [Inject] private GamePlayManager _gamePlayManager;

        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
        }

        public void OnBackgroundClicked()
        {
            _gamePlayManager.OnBackgroundClicked();
        }
    }
}
