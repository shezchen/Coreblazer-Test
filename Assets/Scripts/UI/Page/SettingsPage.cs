using Architecture.GameSound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Architecture
{
    [RequireComponent(typeof(CanvasGroup),typeof(UIBinder))]
    public class SettingsPage:BasePage
    {
        [SerializeField] private float fadeDuration = 0.5f;

        [SerializeField] private TextMeshProUGUI bgmVolume;
        [SerializeField] private TextMeshProUGUI sfxVolume;
        
        [Inject] private SaveManager _saveManager;
        [Inject] private IAudioService _audioService;
        [Inject] private EventBus _eventBus;
        
        private CanvasGroup _canvasGroup;
        private UIBinder _uiBinder;
        
        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
            
            _canvasGroup = GetComponent<CanvasGroup>();
            _uiBinder = GetComponent<UIBinder>();
            
            var finishButton = _uiBinder.Get<Button>("Button_FinishSettings");
            finishButton.OnClickAsObservable().Subscribe( (_) =>
            {
                Hide().Forget();
            });

            _canvasGroup.alpha = 0;
        }

        public override async UniTask Display()
        {
            await _canvasGroup.FadeIn(fadeDuration).AsyncWaitForCompletion();
            _eventBus.Publish(new PageShow(typeof(SettingsPage)));
        }

        public override async UniTask Hide()
        {
            _saveManager.SaveSettings();
            _eventBus.Publish(new PageHide(typeof(SettingsPage)));
            await _canvasGroup.FadeOut(fadeDuration).AsyncWaitForCompletion();
            Destroy(gameObject);
        }
    }
}