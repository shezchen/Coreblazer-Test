using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GamePlay
{
    /// <summary>
    /// 连连看路径渲染器
    /// 负责显示匹配成功时的路径动画（依次亮起效果）
    /// </summary>
    public class PathRenderer : MonoBehaviour
    {
        #region 视觉配置
        
        [Title("视觉配置")]
        [SerializeField, Required, AssetsOnly]
        [LabelText("路径块精灵"), Tooltip("路径高亮块使用的 Sprite")]
        private Sprite _pathSprite;
        
        [SerializeField]
        [LabelText("路径块颜色"), ColorUsage(true, true)]
        private Color _pathColor = new Color(1f, 1f, 0f, 0.8f); // 黄色半透明
        
        [SerializeField, Range(0.3f, 1.5f)]
        [LabelText("路径块缩放"), Tooltip("相对于格子的大小")]
        private float _pathScale = 0.9f;
        
        [SerializeField, Range(-10, 20)]
        [LabelText("路径块层级"), Tooltip("SortingOrder")]
        private int _sortingOrder = 5;
        
        #endregion
        
        #region 动画配置
        
        [Title("动画配置")]
        [SerializeField, Range(0.01f, 0.3f)]
        [LabelText("每格亮起间隔"), Tooltip("路径块依次亮起的时间间隔")]
        private float _lightUpDelay = 0.05f;
        
        [SerializeField, Range(0.05f, 0.5f)]
        [LabelText("渐现时长"), Tooltip("单个路径块渐现动画时长")]
        private float _fadeInDuration = 0.15f;
        
        [SerializeField, Range(0f, 2f)]
        [LabelText("停留时长"), Tooltip("路径完全显示后的停留时间")]
        private float _stayDuration = 0.4f;
        
        [SerializeField, Range(0.1f, 1f)]
        [LabelText("渐隐时长"), Tooltip("路径消失动画时长")]
        private float _fadeOutDuration = 0.25f;
        
        [SerializeField]
        [LabelText("使用缩放动画"), Tooltip("路径块出现时是否有缩放效果")]
        private bool _useScaleAnimation = true;
        
        [SerializeField, ShowIf("_useScaleAnimation"), Range(0.1f, 1f)]
        [LabelText("初始缩放"), Tooltip("缩放动画的起始值")]
        private float _scaleFrom = 0.3f;
        
        #endregion
        
        #region Runtime Data
        
        /// <summary>
        /// 当前路径块列表
        /// </summary>
        private readonly List<GameObject> _currentPathTiles = new List<GameObject>();
        
        /// <summary>
        /// 路径容器
        /// </summary>
        private Transform _pathContainer;
        
        /// <summary>
        /// GridView 引用（用于计算世界坐标）
        /// </summary>
        private GridView _gridView;
        
        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        private bool _isAnimating;
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化路径渲染器
        /// </summary>
        /// <param name="gridView">GridView 引用</param>
        public void Initialize(GridView gridView)
        {
            _gridView = gridView;
            
            // 创建路径容器（挂在 GridView 下，与格子视图同级）
            if (_pathContainer == null)
            {
                var containerObj = new GameObject("PathContainer");
                _pathContainer = containerObj.transform;
                _pathContainer.SetParent(gridView.transform);
                _pathContainer.localPosition = Vector3.zero;
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示路径动画（完整流程：依次亮起 → 停留 → 消失）
        /// </summary>
        /// <param name="path">路径坐标列表</param>
        public async UniTask ShowPathAnimation(List<(int row, int col)> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("[PathRenderer] Path is null or empty.");
                return;
            }
            
            if (_isAnimating)
            {
                Debug.LogWarning("[PathRenderer] Animation is already playing.");
                return;
            }
            
            _isAnimating = true;
            
            try
            {
                // 1. 创建路径块
                CreatePathTiles(path);
                
                // 2. 依次亮起动画
                await AnimateLightUp();
                
                // 3. 停留
                await UniTask.Delay(System.TimeSpan.FromSeconds(_stayDuration));
                
                // 4. 渐隐消失
                await AnimateFadeOut();
                
                // 5. 清理
                ClearPathTiles();
            }
            finally
            {
                _isAnimating = false;
            }
        }
        
        /// <summary>
        /// 立即清除路径（用于中断动画）
        /// </summary>
        public void ClearPath()
        {
            // 停止所有动画
            foreach (var tile in _currentPathTiles)
            {
                if (tile != null)
                {
                    tile.transform.DOKill();
                    var renderer = tile.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.DOKill();
                    }
                }
            }
            
            ClearPathTiles();
            _isAnimating = false;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 创建路径块
        /// </summary>
        private void CreatePathTiles(List<(int row, int col)> path)
        {
            // 清理旧的路径块
            ClearPathTiles();
            
            foreach (var pos in path)
            {
                // 计算世界坐标
                Vector3 worldPos = _gridView.CalculateWorldPosition(pos.row, pos.col);
                
                // 创建路径块
                GameObject tile = new GameObject($"PathTile_{pos.row}_{pos.col}");
                tile.transform.SetParent(_pathContainer);
                tile.transform.position = worldPos;
                
                // 添加 SpriteRenderer
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = _pathSprite;
                renderer.sortingOrder = _sortingOrder;
                renderer.sortingLayerName = "Above";
                
                // 初始状态：透明
                Color startColor = _pathColor;
                startColor.a = 0f;
                renderer.color = startColor;
                
                // 初始缩放
                float targetScale = _pathScale * _gridView.GetCellSize().x;
                if (_useScaleAnimation)
                {
                    tile.transform.localScale = Vector3.one * targetScale * _scaleFrom;
                }
                else
                {
                    tile.transform.localScale = Vector3.one * targetScale;
                }
                
                _currentPathTiles.Add(tile);
            }
            
            Debug.Log($"[PathRenderer] Created {_currentPathTiles.Count} path tiles.");
        }
        
        /// <summary>
        /// 依次亮起动画
        /// </summary>
        private async UniTask AnimateLightUp()
        {
            float targetScale = _pathScale * _gridView.GetCellSize().x;
            
            for (int i = 0; i < _currentPathTiles.Count; i++)
            {
                GameObject tile = _currentPathTiles[i];
                if (tile == null) continue;
                
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                
                // 渐现动画
                renderer.DOColor(_pathColor, _fadeInDuration)
                    .SetEase(Ease.OutQuad);
                
                // 缩放动画（如果启用）
                if (_useScaleAnimation)
                {
                    tile.transform.DOScale(targetScale, _fadeInDuration)
                        .SetEase(Ease.OutBack);
                }
                
                // 等待间隔（最后一个不等待间隔，但等待动画完成）
                if (i < _currentPathTiles.Count - 1)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(_lightUpDelay));
                }
            }
            
            // 等待最后一个动画完成
            await UniTask.Delay(System.TimeSpan.FromSeconds(_fadeInDuration));
        }
        
        /// <summary>
        /// 渐隐动画（所有路径块同时消失）
        /// </summary>
        private async UniTask AnimateFadeOut()
        {
            float targetScale = _pathScale * _gridView.GetCellSize().x;
            
            foreach (GameObject tile in _currentPathTiles)
            {
                if (tile == null) continue;
                
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                
                // 透明度渐隐
                Color transparent = _pathColor;
                transparent.a = 0f;
                renderer.DOColor(transparent, _fadeOutDuration)
                    .SetEase(Ease.InQuad);
                
                // 缩放动画（如果启用）
                if (_useScaleAnimation)
                {
                    tile.transform.DOScale(targetScale * 0.5f, _fadeOutDuration)
                        .SetEase(Ease.InQuad);
                }
            }
            
            await UniTask.Delay(System.TimeSpan.FromSeconds(_fadeOutDuration));
        }
        
        /// <summary>
        /// 清理所有路径块
        /// </summary>
        private void ClearPathTiles()
        {
            foreach (GameObject tile in _currentPathTiles)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
            _currentPathTiles.Clear();
        }
        
        #endregion
        
        #region 生命周期
        
        private void OnDestroy()
        {
            ClearPath();
        }
        
        #endregion
        
        #region Editor 工具
        
#if UNITY_EDITOR
        [Title("测试工具")]
        [Button("测试路径动画"), DisableInEditorMode]
        private async void TestPathAnimation()
        {
            if (_gridView == null)
            {
                Debug.LogWarning("[PathRenderer] GridView is not assigned. Cannot test.");
                return;
            }
            
            // 创建测试路径
            var testPath = new List<(int, int)>
            {
                (2, 2), (2, 3), (2, 4), (3, 4), (4, 4), (5, 4), (5, 5), (5, 6)
            };
            
            Debug.Log("[PathRenderer] Testing path animation...");
            await ShowPathAnimation(testPath);
            Debug.Log("[PathRenderer] Test complete.");
        }
        
        [Button("清除路径"), DisableInEditorMode]
        private void EditorClearPath()
        {
            ClearPath();
        }
#endif
        
        #endregion
    }
}

