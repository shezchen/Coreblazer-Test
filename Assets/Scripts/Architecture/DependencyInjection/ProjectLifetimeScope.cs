using Architecture.GameSound;
using Architecture.GameSound.AudioProvider;
using Architecture.Language;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Architecture
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [BoxGroup("游戏流程管理器"), LabelText("Game Flow Controller"), SerializeField]
        private GameFlowController gameFlowController;
        
        [BoxGroup("音乐与音效"), LabelText("声音库"), SerializeField]
        private AudioCatalog audioCatalog;

        [BoxGroup("音乐与音效"), LabelText("BGM播放器"), SerializeField]
        private AudioSource bgmSource;

        [BoxGroup("音乐与音效"), LabelText("SFX播放器"), SerializeField]
        private AudioSource sfxSource;

        [BoxGroup("UI"), LabelText("UI Root"), SerializeField]
        private UIRoot uiRoot;
        
        protected override void Configure(IContainerBuilder builder)
        {
            ScopeRef.LifetimeScope = this;//静态入口...

            #region 流程控制

            builder.RegisterComponent(gameFlowController);

            #endregion

            #region 事件总线

            builder.Register<EventBus>(Lifetime.Singleton);

            #endregion

            #region 存档系统

            builder.Register<SaveManager>(Lifetime.Singleton);

            #endregion

            #region 声音相关服务

            builder.RegisterInstance(audioCatalog);
            builder.RegisterInstance(new BgmAudioSourceProvider(bgmSource))
                .As<IBgmAudioSourceProvider>();
            builder.RegisterInstance(new SfxAudioSourceProvider(sfxSource))
                .As<ISfxAudioSourceProvider>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();

            #endregion

            #region 语言管理

            builder.Register<LanguageManager>(Lifetime.Singleton);

            #endregion

            #region UI

            builder.RegisterComponent(uiRoot);
            builder.Register<UIManager>(Lifetime.Singleton);

            #endregion

        }
    }
}
