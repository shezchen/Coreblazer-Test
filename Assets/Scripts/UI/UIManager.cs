using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;

namespace Architecture
{
    /// <summary>
    /// 先初始化，之后根据流程控制器来加载UI
    /// </summary>
    public class UIManager:ManagerNeedInitializeBase
    {
        private UIRoot _uiRoot;
        
        private AsyncOperationHandle<GameObject> _handleMainScenePrefab;
        private AsyncOperationHandle<GameObject> _handleLanguagePagePrefab;
        private AsyncOperationHandle<GameObject> _handleSettingsPagePrefab;
        
        public UIManager(UIRoot root)
        {
            _uiRoot = root;
        }

        public override async UniTask Init()
        {
            await base.Init();

            //启动预加载
            _handleMainScenePrefab = Addressables.LoadAssetAsync<GameObject>(AddressableKeys.Assets.MainScenePrefab);
            _handleLanguagePagePrefab =
                Addressables.LoadAssetAsync<GameObject>(AddressableKeys.Assets.LanguagePagePrefab);
            _handleSettingsPagePrefab =
                Addressables.LoadAssetAsync<GameObject>(AddressableKeys.Assets.SettingsPagePrefab);
        }
        
        public async UniTask ShowLanguagePage()
        {
            var go = await _handleLanguagePagePrefab;
            var instance = Object.Instantiate(go, _uiRoot.transform);
            ScopeRef.LifetimeScope.Container.InjectGameObject(instance);
            var page = instance.GetComponent<LanguagePage>();
            await page.Display();
        }

        public async UniTask ShowMainScenePage()
        {
            var go = await _handleMainScenePrefab;
            var instance = Object.Instantiate(go, _uiRoot.transform);
            ScopeRef.LifetimeScope.Container.InjectGameObject(instance);
            var page = instance.GetComponent<MainScenePage>();
            await page.Display();
        }

        public async UniTask ShowSettingsPage()
        {
            var go = await _handleSettingsPagePrefab;
            var instance = Object.Instantiate(go, _uiRoot.transform);
            ScopeRef.LifetimeScope.Container.InjectGameObject(instance);
            var page = instance.GetComponent<SettingsPage>();
            await page.Display();
        }
    }
}