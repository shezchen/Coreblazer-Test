using Architecture;
using Architecture.GameSound;
using GamePlay.Events;
using UnityEngine;
using Sirenix.OdinInspector;
using VContainer;
using DG.Tweening;
using Tools;

namespace GamePlay
{
    /// <summary>
    /// 连连看单个格子的视图组件
    /// 负责显示格子内容、处理点击事件、控制可见性
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridItemView : MonoBehaviour
    {
        #region Sprite Mapping
        
        [Title("Sprite Configuration")]
        [SerializeField, Required]
        [Tooltip("ASide值对应的精灵图数组（索引对应ASide值：2,4,6,8,10）")]
        [InfoBox("请按照ASide可能的值(2,4,6,8,10)配置5个Sprite", InfoMessageType.Info)]
        private Sprite[] _aSideSprites = new Sprite[5];
        
        [SerializeField, Required]
        [Tooltip("BSide值对应的精灵图数组（索引对应BSide值：1,3,5,7,9）")]
        [InfoBox("请按照BSide可能的值(1,3,5,7,9)配置5个Sprite", InfoMessageType.Info)]
        private Sprite[] _bSideSprites = new Sprite[5];
        
        [SerializeField]
        [Tooltip("已消除状态的精灵图（白色，当向上数字为0时显示）")]
        private Sprite _eliminatedSpriteWhite;
        
        [SerializeField]
        [Tooltip("已消除状态的精灵图（黑色，当向上数字为0时显示）")]
        private Sprite _eliminatedSpriteBlack;
        
        [SerializeField]
        [Tooltip("当前显示A面还是B面")]
        private bool _showASide = true;
        
        #endregion

        #region Child Renderers
        
        [Title("Child Renderers")]
        [SerializeField, Required, ChildGameObjectsOnly]
        [Tooltip("内容精灵渲染器（显示ASide/BSide）")]
        private SpriteRenderer _contentRenderer;
        
        [SerializeField, Required, ChildGameObjectsOnly]
        [Tooltip("悬停效果子物体（鼠标进入时激活）")]
        private GameObject _hoverOverlay;
        
        [SerializeField, Required, ChildGameObjectsOnly]
        [Tooltip("选中效果子物体（点击选中后激活）")]
        private GameObject _selectedOverlay;
        
        [SerializeField, ChildGameObjectsOnly]
        [Tooltip("错误边框渲染器（匹配失败时显示红色边框）")]
        private SpriteRenderer _errorBorderRenderer;
        
        #endregion
        
        #region Error Feedback Settings
        
        [Title("Error Feedback")]
        [SerializeField, Range(0.3f, 2f), Tooltip("错误边框渐变消失的时长（秒）")]
        private float _errorBorderFadeDuration = 0.8f;
        
        [SerializeField, Tooltip("错误边框的颜色")]
        private Color _errorBorderColor = Color.red;
        
        #endregion

        #region Runtime Data
        
        /// <summary>
        /// 当前格子的内容数据
        /// </summary>
        private GridContent _currentContent;
        
        /// <summary>
        /// 当前格子的网格坐标
        /// </summary>
        private (int row, int col) _gridPosition;
        
        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool _isSelected = false;
        
        /// <summary>
        /// 目标显示尺寸（从GridView传入）
        /// </summary>
        private Vector2 _targetSize;
        
        /// <summary>
        /// ASide值到数组索引的映射
        /// </summary>
        private static readonly int[] ASideIndexMap = { 2, 4, 6, 8, 10 };
        
        /// <summary>
        /// BSide值到数组索引的映射
        /// </summary>
        private static readonly int[] BSideIndexMap = { 1, 3, 5, 7, 9 };
        
        #endregion

        [Inject] private EventBus _eventBus;
        [Inject] private IAudioService _audioService;

        #region Unity Lifecycle
        
        private void Awake()
        {
            // 验证子物体引用
            if (_contentRenderer == null)
            {
                Debug.LogError($"[GridItemView] ContentRenderer is not assigned on {gameObject.name}!");
            }
            
            if (_hoverOverlay == null)
            {
                Debug.LogWarning($"[GridItemView] HoverOverlay is not assigned on {gameObject.name}");
            }
            
            if (_selectedOverlay == null)
            {
                Debug.LogWarning($"[GridItemView] SelectedOverlay is not assigned on {gameObject.name}");
            }
            
            // 默认禁用所有Overlay
            if (_hoverOverlay != null)
            {
                _hoverOverlay.SetActive(false);
            }
            
            if (_selectedOverlay != null)
            {
                _selectedOverlay.SetActive(false);
            }
            
            // 默认禁用错误边框
            if (_errorBorderRenderer != null)
            {
                _errorBorderRenderer.enabled = false;
            }
            
            ScopeRef.LifetimeScope.Container.Inject(this);
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 初始化格子视图（设置网格坐标和目标尺寸）
        /// </summary>
        /// <param name="position">网格坐标(row, col)</param>
        /// <param name="targetSize">目标显示尺寸</param>
        public void Initialize((int, int) position, Vector2 targetSize)
        {
            _gridPosition = position;
            _targetSize = targetSize;
            _isSelected = false;
            
            // 确保初始状态正确
            if (_hoverOverlay != null)
            {
                _hoverOverlay.SetActive(false);
            }
            
            if (_selectedOverlay != null)
            {
                _selectedOverlay.SetActive(false);
            }
            
            // 确保错误边框被禁用
            if (_errorBorderRenderer != null)
            {
                _errorBorderRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// 更新格子内容显示
        /// </summary>
        /// <param name="content">格子内容数据</param>
        public void UpdateContent(GridContent content)
        {
            _currentContent = content;
            
            // 根据当前显示面选择对应的Sprite
            Sprite targetSprite = GetSpriteForContent(content);
            
            if (_contentRenderer != null && targetSprite != null)
            {
                _contentRenderer.sprite = targetSprite;
                
                // 自动调整scale以匹配目标尺寸（拉伸填充）
                AdjustSpriteScale(targetSprite);
                
                _contentRenderer.enabled = true;
            }
            else if (targetSprite == null)
            {
                Debug.LogWarning($"[GridItemView] Sprite not found for content: ASide={content.ASide}, BSide={content.BSide}");
            }
        }
        
        /// <summary>
        /// 设置格子可见性（用于消除后隐藏）
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_contentRenderer != null)
            {
                _contentRenderer.enabled = visible;
            }
            
            // 隐藏时同时禁用所有Overlay
            if (!visible)
            {
                if (_hoverOverlay != null)
                {
                    _hoverOverlay.SetActive(false);
                }
                
                if (_selectedOverlay != null)
                {
                    _selectedOverlay.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (selected)
            {
                _audioService.PlaySfxAsync("GridItemSelected");
            }
            
            // 激活/禁用选中效果Overlay
            if (_selectedOverlay != null)
            {
                _selectedOverlay.SetActive(selected);
            }
            
            // 选中时隐藏悬停效果（选中状态优先级更高）
            if (selected && _hoverOverlay != null)
            {
                _hoverOverlay.SetActive(false);
            }
        }
        
        /// <summary>
        /// 获取当前网格坐标
        /// </summary>
        public (int, int) GetGridPosition() => _gridPosition;
        
        /// <summary>
        /// 获取当前内容数据
        /// </summary>
        public GridContent GetContent() => _currentContent;
        
        /// <summary>
        /// 获取当前选中状态（供外部查询）
        /// </summary>
        public bool IsSelected() => _isSelected;
        
        /// <summary>
        /// 获取当前向上的数字（用于匹配判定）
        /// </summary>
        /// <returns>向上一面的数字，0表示已消除</returns>
        public int GetUpwardValue() => _showASide ? _currentContent.ASide : _currentContent.BSide;

        public bool GetUpward() => _showASide;
        
        /// <summary>
        /// 判断格子是否已被消除（向上的数字为0）
        /// </summary>
        public bool IsEliminated() => GetUpwardValue() == 0;
        
        /// <summary>
        /// 翻转显示面并刷新内容（仅在未选中时生效）
        /// </summary>
        /// <param name="showASide">是否显示A面</param>
        /// <returns>是否成功翻转（如果已选中则返回false）</returns>
        public bool FlipSide(bool showASide)
        {
            // 如果已被选中，不翻转
            if (_isSelected)
            {
                return false;
            }
            
            // 更新显示面
            _showASide = showASide;
            
            // 刷新内容显示
            UpdateContent(_currentContent);
            
            return true;
        }
        
        /// <summary>
        /// 显示错误边框动画（匹配失败时调用）
        /// 红色边框从完全不透明渐变到完全透明并消失
        /// </summary>
        public void ShowErrorBorder()
        {
            if (_errorBorderRenderer == null)
            {
                Debug.LogWarning($"[GridItemView] ErrorBorderRenderer is not assigned on {gameObject.name}");
                return;
            }
            
            // 先终止该渲染器上之前未完成的渐变动画
            _errorBorderRenderer.KillTweens();
            
            // 设置边框颜色为红色（完全不透明）
            _errorBorderRenderer.color = _errorBorderColor;
            _errorBorderRenderer.enabled = true;
            
            // 渐变透明度从 1 到 0
            _errorBorderRenderer.FadeTo(0f, _errorBorderFadeDuration, Ease.OutQuad)
                .OnComplete(() =>
                {
                    // 渐变完成后禁用边框渲染器
                    if (_errorBorderRenderer != null)
                    {
                        _errorBorderRenderer.enabled = false;
                    }
                });
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// 根据GridContent获取对应的Sprite
        /// </summary>
        private Sprite GetSpriteForContent(GridContent content)
        {
            // 获取当前向上的数字
            int upwardValue = _showASide ? content.ASide : content.BSide;
            
            // 如果向上的数字为0，表示已消除，返回消除状态Sprite
            if (upwardValue == 0)
            {
                return _showASide?_eliminatedSpriteWhite:_eliminatedSpriteBlack;
            }
            
            if (_showASide)
            {
                // 显示A面：查找ASide对应的Sprite
                int index = GetASideIndex(content.ASide);
                if (index >= 0 && index < _aSideSprites.Length)
                {
                    return _aSideSprites[index];
                }
            }
            else
            {
                // 显示B面：查找BSide对应的Sprite
                int index = GetBSideIndex(content.BSide);
                if (index >= 0 && index < _bSideSprites.Length)
                {
                    return _bSideSprites[index];
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 将ASide值映射到数组索引
        /// </summary>
        private int GetASideIndex(int aSideValue)
        {
            for (int i = 0; i < ASideIndexMap.Length; i++)
            {
                if (ASideIndexMap[i] == aSideValue)
                    return i;
            }
            return -1;
        }
        
        /// <summary>
        /// 将BSide值映射到数组索引
        /// </summary>
        private int GetBSideIndex(int bSideValue)
        {
            for (int i = 0; i < BSideIndexMap.Length; i++)
            {
                if (BSideIndexMap[i] == bSideValue)
                    return i;
            }
            return -1;
        }
        
        /// <summary>
        /// 自动调整Sprite的缩放以匹配目标尺寸（拉伸填充）
        /// </summary>
        private void AdjustSpriteScale(Sprite sprite)
        {
            if (sprite == null)
            {
                Debug.LogWarning("[GridItemView] Cannot adjust scale: sprite is null");
                return;
            }
            
            // 获取Sprite在世界空间中的实际尺寸（考虑了Pixels Per Unit）
            float spriteWidth = sprite.bounds.size.x;
            float spriteHeight = sprite.bounds.size.y;
            
            if (spriteWidth <= 0 || spriteHeight <= 0)
            {
                Debug.LogWarning($"[GridItemView] Invalid sprite size: {spriteWidth}x{spriteHeight}");
                return;
            }
            
            // 计算需要的缩放比例以填充目标尺寸
            float scaleX = _targetSize.x / spriteWidth;
            float scaleY = _targetSize.y / spriteHeight;
            
            // 应用拉伸填充（可能会改变宽高比）
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        
        #endregion

        #region Input Handling
        
        /// <summary>
        /// 鼠标进入时显示悬停效果
        /// </summary>
        private void OnMouseEnter()
        {
            // 只有在未选中状态下才显示悬停效果
            if (!_isSelected && _hoverOverlay != null)
            {
                _hoverOverlay.SetActive(true);
            }
        }
        
        /// <summary>
        /// 鼠标离开时隐藏悬停效果
        /// </summary>
        private void OnMouseExit()
        {
            if (_hoverOverlay != null)
            {
                _hoverOverlay.SetActive(false);
            }
        }
        
        /// <summary>
        /// 鼠标点击检测（2D）
        /// </summary>
        private void OnMouseDown()
        {
            // 已消除的格子不可点击
            if (IsEliminated())
            {
                Debug.Log($"[GridItemView] Eliminated tile at ({_gridPosition.row}, {_gridPosition.col}) cannot be selected.");
                return;
            }
            
            if (_isSelected)
            {
                _eventBus.Publish(new GridDeSelectedEvent(_gridPosition));
                return;
            }
            
            // 触发点击事件，连接到GamePlayManager
            _eventBus.Publish(new GridSelectedEvent(_gridPosition, GetUpwardValue()));
            
            Debug.Log($"[GridItemView] Clicked at position ({_gridPosition.row}, {_gridPosition.col}), " +
                      $"Upward Value: {GetUpwardValue()}");
        }
        
        #endregion

        #region Editor Utilities
        
#if UNITY_EDITOR
        [Title("Editor Tools")]
        [Button("Toggle Display Side"), DisableInEditorMode]
        private void ToggleDisplaySide()
        {
            _showASide = !_showASide;
            UpdateContent(_currentContent);
        }
        
        [Button("Test Update Content"), DisableInEditorMode]
        private void TestUpdateContent()
        {
            var testContent = new GridContent
            {
                ASide = 2,
                BSide = 1
            };
            UpdateContent(testContent);
        }
#endif
        
        #endregion
    }
}

