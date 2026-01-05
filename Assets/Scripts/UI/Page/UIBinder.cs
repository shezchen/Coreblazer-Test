using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Architecture
{
    public class UIBinder : MonoBehaviour
    {
        [System.Serializable]
        public class UIWidgetData
        {
            public string ID;
            public GameObject Object;
        }

        [Title("UI 对象注册表")]
        [SerializeField]
        private List<UIWidgetData> _widgetList = new List<UIWidgetData>();

        // 运行时字典，用于快速查找
        private Dictionary<string, GameObject> _widgets;
        
        // 定义前缀列表
        private readonly string[] _validPrefixes = new string[] 
        { 
            "Button_",  // 按钮
            "Text_",  // 文本
            "Image_",  // 图片
            "Slider_",  // 滑动条
            "Toggle_",  // 开关
            "Input_",  // 输入框
            "Panel_",  // 面板
            "Object_"   // 通用对象
        };

        private void Awake()
        {
            InitializeDictionary();
        }

        private void InitializeDictionary()
        {
            if (_widgets != null) return;

            _widgets = new Dictionary<string, GameObject>();
            foreach (var data in _widgetList)
            {
                if (string.IsNullOrEmpty(data.ID)) continue;
                
                if (_widgets.ContainsKey(data.ID))
                {
                    Debug.LogWarning($"[UIBinder] 重复的 ID: {data.ID}，已跳过。");
                    continue;
                }
                _widgets[data.ID] = data.Object;
            }
        }

        /// <summary>
        /// 获取指定 ID 对象上的组件
        /// </summary>
        public T Get<T>(string id) where T : Component
        {
            if (_widgets == null) InitializeDictionary();

            if (_widgets.TryGetValue(id, out var go))
            {
                if (go == null)
                {
                    Debug.LogError($"[UIBinder] ID '{id}' 对应的 GameObject 已被销毁或为空！");
                    return null;
                }

                if (go.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            
                Debug.LogError($"[UIBinder] ID '{id}' ('{go.name}') 上找不到组件类型: {typeof(T).Name}");
                return null;
            }

            Debug.LogError($"[UIBinder] 注册表中找不到 ID: '{id}'");
            return null;
        }

        /// <summary>
        /// 获取 GameObject 本身 (用于 SetActive 等)
        /// </summary>
        public GameObject Get(string id)
        {
            if (_widgets == null) InitializeDictionary();
            return _widgets.GetValueOrDefault(id);
        }

        [Button("自动绑定  Button_  Text_  Image_  Slider_  Toggle_  Input_  Panel_  Object_", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void AutoBindByPrefix()
        {
            _widgetList.Clear();
        
            // 获取所有子物体（包括隐藏的）
            var allTransforms = GetComponentsInChildren<Transform>(true);

            foreach (var t in allTransforms)
            {
                if (t == transform) continue; // 跳过自己

                string name = t.name;
                
                bool isValid = _validPrefixes.Any(prefix => name.StartsWith(prefix));

                if (isValid)
                {
                    if (_widgetList.Any(w => w.ID == name))
                    {
                        Debug.LogWarning($"[UIBinder] 发现重名对象 '{name}'，已跳过。请修正层级中的命名。");
                        continue;
                    }

                    _widgetList.Add(new UIWidgetData { ID = name, Object = t.gameObject });
                }
            }
        
            Debug.Log($"[UIBinder] 绑定完成，共找到 {_widgetList.Count} 个 UI 组件。");
        }
    }
}