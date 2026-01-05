using Architecture.GameSound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Generated;
using R3;
using Sirenix.OdinInspector;
using Tools;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;
using VContainer;
using Button = UnityEngine.UI.Button;

namespace Architecture
{
    /// <summary>
    /// 主界面Page
    /// </summary>
    [RequireComponent(typeof(UIBinder))]
    public class MainScenePage : BasePage
    {
        [BoxGroup("按任意键"),Header("按任意键"), SerializeField] private CanvasGroup pressAnyButton;
        [BoxGroup("按任意键"),Header("偏移量"), SerializeField] private Vector2 slideOffset;
        [BoxGroup("按任意键"),Header("滑动时间"), SerializeField] private float slideDuration;
        
        [BoxGroup("主页面"),Header("主页面"), SerializeField] private CanvasGroup mainSceneContent;
        [BoxGroup("主页面"),Header("主页面出现时间"), SerializeField] private float mainSceneDuration;
        [BoxGroup("主页面"),Header("默认选中按钮"), SerializeField] private Button defaultSelectedButton;
        
        [Inject] private IAudioService _audioService;
        [Inject] private UIManager _uiManager;
        [Inject] private EventBus _eventBus;
        
        private UIBinder _uiBinder;
        
        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
            
            //遮挡事件订阅
            _eventBus.Receive<PageHide>().Subscribe((page) =>
            {
                if (page.PageType == typeof(SettingsPage))
                {
                    Debug.Log("MainScenePage收到SettingsPage隐藏事件，显示自己");
                    gameObject.SetActive(true);
                    defaultSelectedButton.Select();
                }
            }).AddTo(this);
            _eventBus.Receive<PageShow>().Subscribe((page)=>
            {
                if (page.PageType == typeof(SettingsPage))
                {
                    Debug.Log("MainScenePage收到SettingsPage显示事件，隐藏自己");
                    gameObject.SetActive(false);
                }
            }).AddTo(this);
            
            _uiBinder = GetComponent<UIBinder>();
            _uiBinder.Get<Button>("Button_Settings").OnClickAsObservable().Subscribe(async (_) =>
            {
                _audioService.PlaySfxAsync(AudioClipName.SFX.ClickSound);
                await _uiManager.ShowSettingsPage();
            }).AddTo(this);
        }
        
        public override async UniTask Display()
        {
            pressAnyButton.gameObject.SetActive(true);
            var canvasGroup = pressAnyButton;
            var pos = pressAnyButton.transform.localPosition;
            pressAnyButton.transform.localPosition -= (Vector3)slideOffset;
            canvasGroup.alpha = 0;
            var seq = DOTween.Sequence();
            seq.Append(pressAnyButton.transform.LocalMoveTo(pos, slideDuration));
            seq.Join(canvasGroup.FadeIn(slideDuration));
            await seq.AsyncWaitForCompletion();

            InputSystem.onAnyButtonPress.CallOnce((_) =>
            {
                _audioService.PlaySfxAsync(AudioClipName.SFX.ClickSound);
                var canvasGroup = pressAnyButton;
                var pos = pressAnyButton.transform.localPosition;
                var seq = DOTween.Sequence();
                seq.Append(pressAnyButton.transform.LocalMoveTo(pos + (Vector3)slideOffset, slideDuration));
                seq.Join(canvasGroup.FadeOut(slideDuration));
                seq.OnComplete(() =>
                {
                    pressAnyButton.gameObject.SetActive(false);
                    ShowMainScene().Forget();
                });
            });
        }
        
        public override async UniTask Hide()
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 按任意键之后，显示主页面的全部内容
        /// </summary>
        public async UniTask ShowMainScene()
        {
            _eventBus.Publish(new PageShow(typeof(MainScenePage)));
            mainSceneContent.gameObject.SetActive(true);
            mainSceneContent.alpha = 0;
            await mainSceneContent.FadeIn(mainSceneDuration).AsyncWaitForCompletion();
        }
        
        public async UniTask HideMainScene()
        {
            await mainSceneContent.FadeOut(mainSceneDuration).AsyncWaitForCompletion();
            mainSceneContent.gameObject.SetActive(false);
            _eventBus.Publish(new PageHide(typeof(MainScenePage)));
        } 
    }
}