using System;
using Architecture.Language;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using VContainer;

namespace Architecture
{
    /// <summary>
    /// 切换语言会有延迟，所以这里要监听语言切换事件来手动更新UI
    /// </summary>
    [RequireComponent(typeof(UIBinder),typeof(CanvasGroup))]
    public class LanguagePage : BasePage
    {
        [SerializeField] private TextMeshProUGUI languageText;
        [SerializeField] private float fadeDuration = 0.5f;

        [Inject] private EventBus _eventBus;
        [Inject] private UIManager _uiManager;
        private UIBinder _uiBinder;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
            
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _eventBus.Receive<LanguageChangeEvent>().Subscribe((type) =>
            {
                switch (type.NewLanguage)
                {
                    case GameLanguageType.Chinese:
                        languageText.text = "这是正确的语言吗？";
                        break;
                    case GameLanguageType.English:
                        languageText.text = "Is this the correct language?";
                        break;
                    case GameLanguageType.Japanese:
                        languageText.text = "これは正しい言語ですか？";
                        break;
                }
            }).AddTo(this);

            _uiBinder = GetComponent<UIBinder>();
            _uiBinder.Get<Button>("Button_Chinese").OnClickAsObservable().Subscribe(async(_) =>
            {
                _eventBus.Publish(new LanguageConfirmEvent(GameLanguageType.Chinese));
                await Hide();
                _uiManager.ShowMainScenePage().Forget();
            }).AddTo(this);
            _uiBinder.Get<Button>("Button_English").OnClickAsObservable().Subscribe(async(_) =>
            {
                _eventBus.Publish(new LanguageConfirmEvent(GameLanguageType.English));
                await Hide();
                _uiManager.ShowMainScenePage().Forget();
            }).AddTo(this);
            _uiBinder.Get<Button>("Button_Japanese").OnClickAsObservable().Subscribe(async(_) =>
            {
                _eventBus.Publish(new LanguageConfirmEvent(GameLanguageType.Japanese));
                await Hide();
                _uiManager.ShowMainScenePage().Forget();
            }).AddTo(this);
        }

        public override async UniTask Display()
        {
            await _canvasGroup.FadeIn(fadeDuration).AsyncWaitForCompletion();
            _eventBus.Publish(new PageShow(typeof(LanguagePage)));
        }

        public override async UniTask Hide()
        {
            await _canvasGroup.FadeOut(fadeDuration).AsyncWaitForCompletion();
            _eventBus.Publish(new PageHide(typeof(LanguagePage)));
            Destroy(gameObject);
        }
    }
}