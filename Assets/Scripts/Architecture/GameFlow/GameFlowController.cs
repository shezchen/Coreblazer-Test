using System;
using System.Linq;
using Architecture.GameSound;
using Architecture.Language;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Architecture
{
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private bool skipLaunching;
        
        [Inject] private UIManager _uiManager;
        [Inject] private UIRoot _uiRoot;
        [Inject] private SaveManager _saveManager;
        [Inject] private LanguageManager _languageManager;
        [Inject] private IAudioService _audioService;
        [Inject] private EventBus _eventBus;
        [Inject] private GamePlay.GamePlayManager _gamePlayManager;
        private async void Start()
        {
            await _languageManager.Init();
            await _saveManager.Init();
            await _uiManager.Init();

            if (skipLaunching)
            {
                return;
            }
            //此处可以加是否是首次启动的判断
            await _uiManager.ShowLanguagePage();
        }

        [Button("StartGamePlay")]
        public void StartGamePLay()
        {
            _uiRoot.ClearRoot();
            _gamePlayManager.gameObject.SetActive(true);
            _gamePlayManager.ResetManager();//先重置数据层
            _gamePlayManager.LoadGamePlay();

        }

        [Button("ExitGamePlay")]
        public async void ExitGamePlay()
        {
            _uiRoot.ClearRoot();
            _gamePlayManager.ClearRoot();
            await _uiManager.ShowMainScenePage();
        }
    }
}