using DG.Tweening;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;

namespace Architecture.Tip
{
    /// <summary>
    /// 通用弹窗组件，支持弹入/缩出动画，隐藏后自动销毁。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PopUp : MonoBehaviour
    {
        [Title("动画设置")]
        [SerializeField, Range(0.1f, 2f), Tooltip("弹入动画时长（秒）")]
        private float _showDuration = 0.25f;
        
        [SerializeField, Range(0.1f, 2f), Tooltip("缩出动画时长（秒）")]
        private float _hideDuration = 0.2f;
        
        [SerializeField, Range(0f, 1f), Tooltip("弹入起始缩放比例（0 = 从无到有，0.8 = 从80%到100%）")]
        private float _initialScale = 0.8f;
        
        [SerializeField, Range(0f, 1f), Tooltip("缩出目标缩放比例（0 = 缩到消失，0.8 = 缩到80%）")]
        private float _finalScale = 0.8f;
        
        [SerializeField, Tooltip("是否独立于 Time.timeScale 更新（暂停时仍播放）")]
        private bool _independentUpdate = true;
        
        [Title("行为设置")]
        [SerializeField, Tooltip("显示后自动隐藏")]
        private bool _autoHideAfterShow = false;
        
        [SerializeField, ShowIf("_autoHideAfterShow"), Range(0.5f, 10f), Tooltip("自动隐藏延迟（秒）")]
        private float _autoHideDelay = 3f;
        
        [Title("初始化设置")]
        [SerializeField, Tooltip("启动时自动播放弹入动画")]
        private bool _showOnEnable = true;
        
        private CanvasGroup _canvasGroup;
        private Sequence _activeSequence;
        private Vector3 _originalScale;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalScale = transform.localScale;
            
            // 初始状态设为不可见
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (_showOnEnable)
            {
                Show();
            }
        }

        private void OnDisable()
        {
            // 清理所有 Tweens
            this.KillTweens();
            _activeSequence?.Kill();
            _activeSequence = null;
        }

        /// <summary>
        /// 播放弹入动画（渐显 + 缩放）。
        /// </summary>
        [Button("显示弹窗"), DisableInEditorMode]
        public void Show()
        {
            // 终止之前的动画
            _activeSequence?.Kill();
            
            // 恢复原始缩放并应用起始缩放
            transform.localScale = _originalScale * _initialScale;
            
            // 使用 DOTweenTool 的组合动画
            _activeSequence = _canvasGroup.ShowWithFadeAndPop(transform, _showDuration);
            
            if (_activeSequence != null)
            {
                if (_independentUpdate)
                {
                    _activeSequence.SetUpdate(true);
                }
                
                _activeSequence.OnComplete(() =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                    
                    // 自动隐藏逻辑
                    if (_autoHideAfterShow)
                    {
                        DOVirtual.DelayedCall(_autoHideDelay, Hide)
                            .SetUpdate(_independentUpdate)
                            .SetTarget(this);
                    }
                });
            }
        }

        /// <summary>
        /// 播放缩出动画（渐隐 + 缩放），完成后销毁自身。
        /// </summary>
        [Button("隐藏弹窗"), DisableInEditorMode]
        public void Hide()
        {
            // 终止之前的动画
            _activeSequence?.Kill();
            this.KillTweens(); // 清理可能的延迟调用
            
            // 禁用交互
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            // 使用 DOTweenTool 的组合动画
            _activeSequence = _canvasGroup.HideWithFadeAndShrink(transform, _hideDuration);
            
            if (_activeSequence != null)
            {
                if (_independentUpdate)
                {
                    _activeSequence.SetUpdate(true);
                }
                
                // 重写目标缩放为自定义的 finalScale
                _activeSequence.Kill();
                _activeSequence = DOTween.Sequence()
                    .Append(transform.DOScale(_originalScale * _finalScale, _hideDuration).SetEase(Ease.InQuad))
                    .Join(_canvasGroup.DOFade(0f, _hideDuration))
                    .SetTarget(_canvasGroup);
                
                if (_independentUpdate)
                {
                    _activeSequence.SetUpdate(true);
                }
                
                _activeSequence.OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            }
            else
            {
                // 如果动画创建失败，直接销毁
                Destroy(gameObject);
            }
        }
    }
}