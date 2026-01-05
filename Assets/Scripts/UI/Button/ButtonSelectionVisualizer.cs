using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architecture
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonSelectionVisualizer : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("当按钮被选中时显示的物体")]
        [SerializeField] private GameObject selectionIndicator;

        [Tooltip("是否在开始时自动隐藏指示器？")]
        [SerializeField] private bool hideOnAwake = true;
        
        [Header("加载时直接选中"), SerializeField] private bool selectOnAwake = false;

        private void Awake()
        {
            if (selectionIndicator == null)
            {
                Debug.LogWarning($"[{nameof(ButtonSelectionVisualizer)}] 未设置选择指示器，组件将无法正常工作。", this);
                return;
            }

            if (hideOnAwake && selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }

            if (selectOnAwake)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(true);
            }
        }
        
        public void OnDeselect(BaseEventData eventData)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            if (selectionIndicator != null) selectionIndicator.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (selectionIndicator != null) selectionIndicator.SetActive(false);
        }
        
    }
}