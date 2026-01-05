using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer;
using R3;

namespace Architecture.Language
{
    /// <summary>
    /// 用来切换语言，CurrentLanguage储存现在使用的语言
    /// </summary>
    public class LanguageManager:ManagerNeedInitializeBase
    {
        [Inject] private EventBus _eventBus;
        
        public static GameLanguageType CurrentLanguage { get; private set; } = GameLanguageType.English;
        private bool _isInitialized = false;
        
        private const string CodeEn = "en";
        private const string CodeZh = "zh-Hans";
        private const string CodeJa = "ja";
        
        /// <summary>
        /// 游戏开始时需要使用此方法初始化LanguageManager
        /// </summary>
        public override async UniTask Init()
        {
            await base.Init();
            Debug.Log("LanguageManager启动");
            await StartInitialize();

            _eventBus.Receive<LanguageConfirmEvent>().Subscribe(language =>
            {
                SetToLanguage(language.ConfirmedLanguage).Forget();
            });
        }

        private async UniTask StartInitialize()
        {
            try
            {
                if (_isInitialized)
                    return;
                // 等待 Localization Settings 初始化完成
                await LocalizationSettings.InitializationOperation.Task;
                switch (LocalizationSettings.SelectedLocale.Identifier.Code)
                {
                    case CodeEn:
                        CurrentLanguage = GameLanguageType.English;
                        break;
                    case CodeZh:
                        CurrentLanguage = GameLanguageType.Chinese;
                        break;
                    case CodeJa:
                        CurrentLanguage = GameLanguageType.Japanese;
                        break;
                }
                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError("语言管理服务异常："+e.Message);
            }
        }
        
        public async UniTask SetToLanguage(GameLanguageType type)
        {
            try
            {
                switch (type)
                {
                    case GameLanguageType.Chinese:
                        await ChangeLocaleByIdentifier(CodeZh);
                        break;
                    case GameLanguageType.English:
                        await ChangeLocaleByIdentifier(CodeEn);
                        break;
                    case GameLanguageType.Japanese:
                        await ChangeLocaleByIdentifier(CodeJa);
                        break;
                }
                CurrentLanguage = type;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
        }


        /// <summary>
        /// 通过语言代码（例如 "en", "zh-Hans"）来切换语言
        /// </summary>
        /// <param name="code">语言代码</param>
        private async UniTask ChangeLocaleByIdentifier(string code)
        {
            Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (targetLocale != null)
            {
                LocalizationSettings.SelectedLocale = targetLocale;
                await LocalizationSettings.InitializationOperation.ToUniTask(); 
            }
            else
            {
                Debug.LogWarning($"未找到语言代码为 '{code}' 的 Locale。");
            }
        }
    }
}