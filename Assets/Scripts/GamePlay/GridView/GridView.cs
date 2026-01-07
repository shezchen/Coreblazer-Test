using System.Collections.Generic;
using Architecture;
using GamePlay.Events;
using UnityEngine;
using Sirenix.OdinInspector;
using VContainer;

namespace GamePlay
{
    /// <summary>
    /// 连连看网格视图管理器
    /// 负责创建和管理10x10的固定网格，并根据BoardDictionary更新格子内容
    /// </summary>
    public class GridView : MonoBehaviour
    {
        #region Layout Settings
        
        [Title("Grid Layout Settings")]
        [SerializeField, LabelText("单元格大小")]
        [Tooltip("单个格子的尺寸")]
        private Vector2 _cellSize = new Vector2(1f, 1f);
        
        [SerializeField, LabelText("格子间距")]
        [Tooltip("格子之间的间距")]
        private Vector2 _spacing = new Vector2(0.1f, 0.1f);
        
        #endregion

        #region References
        
        [Title("Prefab References")]
        [SerializeField, Required, AssetsOnly]
        [Tooltip("格子预制体（必须包含GridItemView组件）")]
        private GameObject _gridItemPrefab;
        
        [SerializeField, ChildGameObjectsOnly]
        [Tooltip("网格容器节点（为空则使用自身作为父节点）")]
        private Transform _gridContainer;
        
        #endregion

        #region Runtime Data

        /// <summary>
        /// 所有格子视图的引用字典，键为(row, col)坐标
        /// </summary>
        public readonly Dictionary<(int, int), GridItemView> GridItems =
            new Dictionary<(int, int), GridItemView>(GRID_SIZE * GRID_SIZE);
        
        /// <summary>
        /// 是否已经初始化网格
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// 网格尺寸（10x10固定）
        /// </summary>
        private const int GRID_SIZE = 10;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            // 如果未指定容器，使用自身作为容器
            if (_gridContainer == null)
            {
                _gridContainer = transform;
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 渲染网格（首次调用会初始化，后续仅更新内容）
        /// </summary>
        /// <param name="boardData">棋盘数据字典</param>
        public void RenderGrid(in Dictionary<(int, int), GridContent> boardData)
        {
            if (!_isInitialized)
            {
                InitializeGrid();
            }
            
            UpdateGrid(boardData);
        }
        
        /// <summary>
        /// 获取指定坐标的格子视图
        /// </summary>
        public GridItemView GetGridItem(int row, int col)
        {
            if (GridItems != null && GridItems.TryGetValue((row, col), out var item))
            {
                return item;
            }
            return null;
        }
        
        /// <summary>
        /// 翻转所有未选中的格子
        /// </summary>
        /// <param name="showASide">是否显示A面</param>
        public void FlipAllTiles(bool showASide)
        {
            if (GridItems == null)
            {
                Debug.LogWarning("[GridView] Cannot flip tiles: grid is not initialized.");
                return;
            }
            
            int flippedCount = 0;
            foreach (var kvp in GridItems)
            {
                var itemView = kvp.Value;
                if (itemView.FlipSide(showASide))
                {
                    flippedCount++;
                }
            }
            
            Debug.Log($"[GridView] Flipped {flippedCount} tiles to {(showASide ? "A-Side" : "B-Side")}");
        }
        
        /// <summary>
        /// 计算网格坐标对应的世界坐标（中心对齐）
        /// 支持外圈虚拟格子（row/col 为 -1 或 10）
        /// </summary>
        /// <param name="row">行号（-1 到 10）</param>
        /// <param name="col">列号（-1 到 10）</param>
        /// <returns>世界坐标</returns>
        public Vector3 CalculateWorldPosition(int row, int col)
        {
            // 计算中心对齐的偏移量
            float pivotOffsetX = (GRID_SIZE - 1) * 0.5f;
            float pivotOffsetY = (GRID_SIZE - 1) * 0.5f;
            
            // 计算相对于中心的坐标
            float x = (col - pivotOffsetX) * (_cellSize.x + _spacing.x);
            float y = (row - pivotOffsetY) * (_cellSize.y + _spacing.y);
            
            // 加上 Transform 的世界坐标
            return _gridContainer.position + new Vector3(x, y, 0);
        }
        
        /// <summary>
        /// 获取单元格尺寸（供 PathRenderer 使用）
        /// </summary>
        public Vector2 GetCellSize() => _cellSize;
        
        /// <summary>
        /// 获取格子间距（供 PathRenderer 使用）
        /// </summary>
        public Vector2 GetSpacing() => _spacing;
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// 初始化网格：创建100个GridItemView实例并设置位置
        /// </summary>
        private void InitializeGrid()
        {
            if (_gridItemPrefab == null)
            {
                Debug.LogError("[GridView] GridItemPrefab is null! Please assign a prefab in the inspector.");
                return;
            }
            
            GridItems.Clear();
            
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    // 计算世界坐标
                    Vector3 worldPos = CalculateWorldPosition(row, col);
                    
                    // 实例化预制体
                    GameObject itemObj = Instantiate(_gridItemPrefab, worldPos, Quaternion.identity, _gridContainer);
                    itemObj.name = $"GridItem_{row}_{col}";
                    
                    // 获取GridItemView组件并初始化（传入目标尺寸）
                    GridItemView itemView = itemObj.GetComponent<GridItemView>();
                    if (itemView == null)
                    {
                        Debug.LogError($"[GridView] Prefab '{_gridItemPrefab.name}' does not have GridItemView component!");
                        Destroy(itemObj);
                        continue;
                    }
                    
                    itemView.Initialize((row, col), _cellSize);
                    GridItems[(row, col)] = itemView;
                }
            }
            
            _isInitialized = true;
            Debug.Log($"[GridView] Grid initialized with {GridItems.Count} items.");
        }
        
        /// <summary>
        /// 更新网格内容：遍历所有GridItemView并更新其显示内容
        /// </summary>
        private void UpdateGrid(Dictionary<(int, int), GridContent> boardData)
        {
            if (GridItems == null || boardData == null)
            {
                return;
            }
            
            foreach (var kvp in GridItems)
            {
                var position = kvp.Key;
                var itemView = kvp.Value;
                
                if (boardData.TryGetValue(position, out GridContent content))
                {
                    // TODO: 调用GridItemView的UpdateContent方法更新显示
                    itemView.UpdateContent(content);
                }
                else
                {
                    // 如果数据中没有这个位置的内容，隐藏该格子
                    itemView.SetVisible(false);
                }
            }
        }
        
        #endregion

        #region Editor Utilities
        
#if UNITY_EDITOR
        /// <summary>
        /// 在Scene视图中绘制网格布局预览
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_gridContainer == null)
                return;
            
            Gizmos.color = Color.cyan;
            
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    Vector3 pos = CalculateWorldPosition(row, col);
                    Gizmos.DrawWireCube(pos, new Vector3(_cellSize.x, _cellSize.y, 0.1f));
                }
            }
        }
#endif
        
        #endregion
    }
}

