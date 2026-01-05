using System;
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
            
            var resolutionDropdown = _uiBinder.Get<TMP_Dropdown>("Object_Resolution");
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(option =>
                ("Res_" + option.text) == _saveManager.CurrentSettingsSave.gameResolution.ToString());
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(async (index) =>
            {
                var options = resolutionDropdown.options;
                if (index >= 0 && index < options.Count)
                {
                    var resText = options[index].text;
                    
                    var dimensions = resText.Split('x');
                    if (dimensions.Length == 2 &&
                        int.TryParse(dimensions[0].Trim(), out int width) &&
                        int.TryParse(dimensions[1].Trim(), out int height))
                    {
                        Screen.SetResolution(width, height,
                            _saveManager.CurrentSettingsSave.gameWindow == GameWindow.FullScreenWindow
                                ? FullScreenMode.FullScreenWindow
                                : FullScreenMode.Windowed);

                        var currentAspect = (float)width / height;
                        const float targetAspect = 16f / 9f; // 目标宽高比16:9
                        
                        await UniTask.Yield();
                    }
                    
                    _saveManager.CurrentSettingsSave.gameResolution = resText switch
                    {
                        "1280x720" => GameResolution.Res_1280x720,
                        "1366x768" => GameResolution.Res_1366x768,
                        "1600x900" => GameResolution.Res_1600x900,
                        "1920x1080" => GameResolution.Res_1920x1080,
                        "2560x1440" => GameResolution.Res_2560x1440,
                        "3840x2160" => GameResolution.Res_3840x2160,
                        "1280x800" => GameResolution.Res_1280x800,
                        "1920x1200" => GameResolution.Res_1920x1200,
                        "2560x1600" => GameResolution.Res_2560x1600,
                        _ => _saveManager.CurrentSettingsSave.gameResolution
                    };
                }
            });
            
            var fullScreenToggle = _uiBinder.Get<Toggle>("Toggle_FullScreen");
            fullScreenToggle.isOn = _saveManager.CurrentSettingsSave.gameWindow == GameWindow.FullScreenWindow;
            fullScreenToggle.onValueChanged.RemoveAllListeners();
            fullScreenToggle.onValueChanged.AddListener((isFullScreen) =>
            {
                Screen.fullScreenMode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                _saveManager.CurrentSettingsSave.gameWindow = isFullScreen
                    ? GameWindow.FullScreenWindow
                    : GameWindow.Window;
            });
            
            var bgmSlider = _uiBinder.Get<Slider>("Slider_BGM");
            bgmSlider.value = _saveManager.CurrentSettingsSave.bgmVolume;
            bgmVolume.text = Mathf.RoundToInt(bgmSlider.value).ToString();
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener((value) =>
            {
                _audioService.SetBgmVolume(value / 100f);
                bgmVolume.text = Mathf.RoundToInt(value).ToString();
                _saveManager.CurrentSettingsSave.bgmVolume = Mathf.RoundToInt(value);
            });
            
            var sfxSlider = _uiBinder.Get<Slider>("Slider_SFX");
            sfxSlider.value = _saveManager.CurrentSettingsSave.sfxVolume;
            sfxVolume.text = Mathf.RoundToInt(sfxSlider.value).ToString();
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener((value) =>
            {
                _audioService.SetSfxVolume(value / 100f);
                sfxVolume.text = Mathf.RoundToInt(value).ToString();
                _saveManager.CurrentSettingsSave.sfxVolume = Mathf.RoundToInt(value);
            });
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