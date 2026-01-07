using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using DG.Tweening;
using Tools;

namespace Architecture
{
    /// <summary>
    /// 响应输入系统切换页面背景颜色（黑白渐变）的组件。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ChangePage : MonoBehaviour
    {
        #region Settings

        [Title("Transition Settings")]
        [SerializeField, Range(0.1f, 2f), Tooltip("颜色渐变的时长（秒）")]
        private float _fadeDuration = 0.5f;

        [SerializeField, Tooltip("渐变缓动曲线")]
        private Ease _fadeEase = Ease.OutQuad;

        #endregion

        #region Colors

        [Title("Color Settings")]
        [SerializeField, Tooltip("白色主题颜色")]
        private Color _whiteColor = Color.white;

        [SerializeField, Tooltip("黑色主题颜色")]
        private Color _blackColor = Color.black;

        #endregion

        #region Debug Info

        [Title("Runtime Status")]
        [ShowInInspector, ReadOnly, Tooltip("当前是否为白色状态")]
        private bool _currentIsWhite = true;

        #endregion

        #region Private Fields

        private InputSystem_Actions _inputSystemActions;
        private SpriteRenderer _sprite;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 创建并启用输入系统
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Enable();

            // 缓存 Image 组件
            _sprite = GetComponent<SpriteRenderer>();

            // 初始化为白色
            _sprite.color = _whiteColor;
            _currentIsWhite = true;
        }

        private void OnEnable()
        {
            // 订阅输入事件
            _inputSystemActions.UI.TrunGrid.performed += TurnGrid;
        }

        private void OnDisable()
        {
            // 取消订阅输入事件
            _inputSystemActions.UI.TrunGrid.performed -= TurnGrid;

            // 清理 DOTween 动画
            this.KillTweens();
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// 响应 TurnGrid 输入，切换黑白颜色。
        /// </summary>
        private void TurnGrid(InputAction.CallbackContext context)
        {
            // 切换状态
            _currentIsWhite = !_currentIsWhite;

            // 确定目标颜色
            Color targetColor = _currentIsWhite ? _whiteColor : _blackColor;

            // 先终止之前未完成的渐变动画（防止重叠）
            this.KillTweens();

            // 使用 DOTweenTool 进行颜色渐变
            _sprite.ColorTo(targetColor, _fadeDuration, _fadeEase);
        }

        #endregion

        #region Editor Buttons

        [Title("Editor Testing")]
        [Button("Test Toggle Color", ButtonSizes.Medium)]
        [GUIColor(0.3f, 0.8f, 1f)]
        private void TestToggleColor()
        {
            if (_sprite == null)
            {
                _sprite = GetComponent<SpriteRenderer>();
            }

            _currentIsWhite = !_currentIsWhite;
            Color targetColor = _currentIsWhite ? _whiteColor : _blackColor;

            if (Application.isPlaying)
            {
                this.KillTweens();
                _sprite.ColorTo(targetColor, _fadeDuration, _fadeEase);
            }
            else
            {
                _sprite.color = targetColor;
            }
        }

        #endregion
    }
}