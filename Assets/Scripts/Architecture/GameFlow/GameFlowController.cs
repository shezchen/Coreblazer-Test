using System;
using System.Linq;
using Architecture.GameSound;
using Architecture.Language;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Architecture
{
    public class GameFlowController : MonoBehaviour
    {
        [Inject] private UIManager _uiManager;
        [Inject] private SaveManager _saveManager;
        [Inject] private LanguageManager _languageManager;
        [Inject] private IAudioService _audioService;
        [Inject] private EventBus _eventBus;
        private async void Start()
        {
            await _languageManager.Init();
            await _saveManager.Init();
            await _uiManager.Init();

            //此处可以加是否是首次启动的判断
            await _uiManager.ShowLanguagePage();
        }
    }
    

}