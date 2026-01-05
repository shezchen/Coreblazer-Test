using System;
using System.Collections.Generic;
using Architecture;
using Cysharp.Threading.Tasks;
using GamePlay.Events;
using R3;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Random = System.Random;

namespace GamePlay
{
    public class GamePlayManager:SerializedMonoBehaviour
    {
        #region 视图相关
        
        [Title("视图配置")]
        [SerializeField, Required, AssetsOnly]
        [LabelText("网格视图预制体"), Tooltip("GridView 预制体引用")]
        private GridView _gridViewPrefab;
        
        [SerializeField, ChildGameObjectsOnly]
        [LabelText("网格父节点"), Tooltip("GridView 实例化的父节点，为空则使用当前 Transform")]
        private Transform _gridViewParent;
        
        /// <summary>
        /// 当前的网格视图实例
        /// </summary>
        private GridView _currentGridView;
        
        #endregion
        
        #region 数据相关
        
        public readonly Dictionary<(int, int), GridContent> BoardDictionary = new();
        [ShowInInspector,ReadOnly] public bool CurrentUpside { get; private set; }
        
        /// <summary>
        /// 之前选择的网格坐标（null 表示没有选择）
        /// </summary>
        [ShowInInspector, ReadOnly]
        private (int row, int col)? _previousSelectedPosition = null;
        
        #endregion

        [Inject] private EventBus _eventBus;
        private InputSystem_Actions _inputSystemActions;
        
        /// <summary>
        /// 事件订阅管理器（用于在 OnDisable 时统一取消订阅）
        /// </summary>
        private DisposableBag _disposableBag;
        
        private void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Enable();
        }

        private void OnEnable()
        {
            _disposableBag = new DisposableBag();
            
            _inputSystemActions.UI.TrunGrid.performed += TurnGrid;
            
            _eventBus.Receive<GridSelectedEvent>()
                .Subscribe(OnGridSelected)
                .AddTo(ref _disposableBag);
            _eventBus.Receive<GridDeSelectedEvent>()
                .Subscribe(OnGridDeSelected)
                .AddTo(ref _disposableBag);
        }

        private void OnDisable()
        {
            _inputSystemActions.UI.TrunGrid.performed -= TurnGrid;
            
            _disposableBag.Dispose();
        }

        public void ResetManager()
        {
            SetBoard();
            CurrentUpside = true;
            _previousSelectedPosition = null;
        }
        
        /// <summary>
        /// 加载游戏玩法视图
        /// </summary>
        public void LoadGamePlay()
        {
            ClearRoot();
            // 实例化网格视图预制体
            _currentGridView = Instantiate(_gridViewPrefab, _gridViewParent != null ? _gridViewParent : transform);
            _currentGridView.transform.localPosition = Vector3.zero;
            
            // 渲染网格数据
            _currentGridView.RenderGrid(in BoardDictionary);
        }

        public void ClearRoot()
        {
            foreach (Transform t in _gridViewParent != null ? _gridViewParent : transform)
            {
                Destroy(t.gameObject);
            }
        }

        private void SetBoard()
        {
            var random = new Random();

            var validPairs = new List<(int even, int odd)>(100);

            var validOddsForEven = new Dictionary<int, int[]>
            {
                { 2, new[] { 1, 3, 5, 7 } },
                { 4, new[] { 1, 3, 5, 9 } },
                { 6, new[] { 1, 3, 7, 9 } },
                { 8, new[] { 1, 5, 7, 9 } },
                { 10, new[] { 3, 5, 7, 9 } }
            };

            foreach (var kvp in validOddsForEven)
            {
                int even = kvp.Key;
                int[] validOdds = kvp.Value;
                int oddsPerType = 20 / validOdds.Length;

                foreach (var odd in validOdds)
                {
                    for (int i = 0; i < oddsPerType; i++)
                    {
                        validPairs.Add((even, odd));
                    }
                }
            }

            ShufflePairs(validPairs, random);

            BoardDictionary.Clear();
            
            int pairIndex = 0;
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    var (even, odd) = validPairs[pairIndex];
                    
                    var gridContent = new GridContent
                    {
                        ASide = even,
                        BSide = odd
                    };
                    BoardDictionary[(row, col)] = gridContent;
                    
                    pairIndex++;
                }
            }
        }
        
        private void ShufflePairs(List<(int, int)> pairs, Random random)
        {
            int n = pairs.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pairs[i], pairs[j]) = (pairs[j], pairs[i]);
            }
        }

        private void TurnGrid(InputAction.CallbackContext context)
        {
            CurrentUpside = !CurrentUpside;
            
            // 翻转网格中所有未选中的格子
            if (_currentGridView != null)
            {
                _currentGridView.FlipAllTiles(CurrentUpside);
            }
            else
            {
                Debug.LogWarning("[GamePlayManager] Cannot flip tiles: GridView is not loaded yet.");
            }
        }
        
        #region 格子选择处理
        
        /// <summary>
        /// 处理格子被选中事件
        /// </summary>
        private void OnGridSelected(GridSelectedEvent evt)
        {
            Debug.Log($"[GamePlayManager] Grid selected at ({evt.Position.Item1}, {evt.Position.Item2}), Content: {evt.Content}");
            
            // 设置格子的选中状态
            var selectedItem = _currentGridView?.GetGridItem(evt.Position.Item1, evt.Position.Item2);
            if (selectedItem != null)
            {
                selectedItem.SetSelected(true);
            }
            
            // 如果之前没有选中的格子，保存当前选中的坐标
            if (_previousSelectedPosition == null)
            {
                _previousSelectedPosition = evt.Position;
                Debug.Log($"[GamePlayManager] First selection saved: {evt.Position}");
            }
            else
            {
                // 如果之前已经有选中的格子，进入连连看判定
                Debug.Log($"[GamePlayManager] Second selection detected. Previous: {_previousSelectedPosition}, Current: {evt.Position}");
                CheckMatch(_previousSelectedPosition.Value, evt.Position);
            }
        }
        
        /// <summary>
        /// 处理格子被取消选中事件（二次点击）
        /// </summary>
        private void OnGridDeSelected(GridDeSelectedEvent evt)
        {
            Debug.Log($"[GamePlayManager] Grid deselected at ({evt.Position.Item1}, {evt.Position.Item2})");
            
            // 取消格子的选中状态
            var deselectedItem = _currentGridView?.GetGridItem(evt.Position.Item1, evt.Position.Item2);
            if (deselectedItem != null)
            {
                deselectedItem.SetSelected(false);
            }
            
            // 清除之前选中的坐标
            if (_previousSelectedPosition.HasValue && _previousSelectedPosition.Value == evt.Position)
            {
                _previousSelectedPosition = null;
                Debug.Log($"[GamePlayManager] Selection cleared.");
            }
        }
        
        /// <summary>
        /// 连连看判定方法
        /// </summary>
        /// <param name="pos1">第一个格子的坐标</param>
        /// <param name="pos2">第二个格子的坐标</param>
        private void CheckMatch((int, int) pos1, (int, int) pos2)
        {
            Debug.Log($"[GamePlayManager] CheckMatch called: {pos1} vs {pos2}");
            
            // 1. 获取两个格子向上的数字
            int value1 = GetUpwardValue(pos1);
            int value2 = GetUpwardValue(pos2);
            
            Debug.Log($"[GamePlayManager] Values: {value1} + {value2} = {value1 + value2}");
            
            // 2. 检查数字之和是否等于 11
            if (value1 + value2 != 11)
            {
                Debug.Log($"[GamePlayManager] Match failed: sum is not 11");
                OnMatchFailed(pos1, pos2);
                return;
            }
            
            // 3. 检查是否存在有效路径
            bool hasPath = LinkPathFinder.HasValidPath(pos1, pos2, IsPositionPassable);
            
            if (hasPath)
            {
                Debug.Log($"[GamePlayManager] Match success! Valid path found.");
                OnMatchSuccess(pos1, pos2);
            }
            else
            {
                Debug.Log($"[GamePlayManager] Match failed: no valid path.");
                OnMatchFailed(pos1, pos2);
            }
        }
        
        #endregion
        
        #region 匹配结果处理
        
        /// <summary>
        /// 匹配成功：消除两个格子
        /// </summary>
        /// <param name="pos1">第一个格子的坐标</param>
        /// <param name="pos2">第二个格子的坐标</param>
        private void OnMatchSuccess((int, int) pos1, (int, int) pos2)
        {
            // 1. 消除第一个格子
            EliminateTile(pos1);
            
            // 2. 消除第二个格子
            EliminateTile(pos2);
            
            // 3. 取消两个格子的选中状态
            ClearTileSelection(pos1);
            ClearTileSelection(pos2);
            
            // 4. 清空之前选中的坐标
            _previousSelectedPosition = null;
            
            Debug.Log($"[GamePlayManager] Tiles eliminated: {pos1} and {pos2}");
        }
        
        /// <summary>
        /// 匹配失败：取消选中状态
        /// </summary>
        /// <param name="pos1">第一个格子的坐标</param>
        /// <param name="pos2">第二个格子的坐标</param>
        private void OnMatchFailed((int, int) pos1, (int, int) pos2)
        {
            // 1. 取消两个格子的选中状态
            ClearTileSelection(pos1);
            ClearTileSelection(pos2);
            
            // 2. 清空之前选中的坐标
            _previousSelectedPosition = null;
            
            Debug.Log($"[GamePlayManager] Match failed, selections cleared.");
        }
        
        /// <summary>
        /// 消除指定位置的格子（将向上的数字置为0）
        /// 此方法可用于后期添加消除动画/音效
        /// </summary>
        /// <param name="pos">格子坐标</param>
        private void EliminateTile((int, int) pos)
        {
            // 更新数据层：将向上的数字置为0
            SetUpwardValue(pos, 0);
            
            // 更新视图层：刷新格子显示
            RefreshTileView(pos);
            
            // TODO: 在此处添加消除动画
            // TODO: 在此处添加消除音效
        }
        
        /// <summary>
        /// 取消指定格子的选中状态
        /// 此方法可用于后期添加取消选中动画/音效
        /// </summary>
        /// <param name="pos">格子坐标</param>
        private void ClearTileSelection((int, int) pos)
        {
            var item = _currentGridView?.GetGridItem(pos.Item1, pos.Item2);
            if (item != null)
            {
                item.SetSelected(false);
            }
            
            // TODO: 在此处添加取消选中动画/音效
        }
        
        /// <summary>
        /// 刷新指定格子的视图显示
        /// 此方法可用于后期添加刷新动画
        /// </summary>
        /// <param name="pos">格子坐标</param>
        private void RefreshTileView((int, int) pos)
        {
            var item = _currentGridView?.GetGridItem(pos.Item1, pos.Item2);
            if (item != null && BoardDictionary.TryGetValue(pos, out var content))
            {
                item.UpdateContent(content);
            }
            
            // TODO: 在此处添加刷新动画
        }
        
        #endregion
        
        #region 数据访问辅助方法
        
        /// <summary>
        /// 获取指定位置格子向上的数字
        /// </summary>
        /// <param name="pos">格子坐标</param>
        /// <returns>向上一面的数字，0表示已消除</returns>
        private int GetUpwardValue((int, int) pos)
        {
            if (!BoardDictionary.TryGetValue(pos, out var content))
            {
                return 0;
            }
            
            return  _currentGridView.GridItems[pos].GetUpward() ? content.ASide : content.BSide;
        }
        
        /// <summary>
        /// 设置指定位置格子向上的数字
        /// </summary>
        /// <param name="pos">格子坐标</param>
        /// <param name="value">要设置的值</param>
        private void SetUpwardValue((int, int) pos, int value)
        {
            if (!BoardDictionary.TryGetValue(pos, out var content))
            {
                return;
            }
            
            // 使用格子实际的朝向，而不是全局的 CurrentUpside
            // 因为被选中的格子可能与其他格子朝向不一致
            var item = _currentGridView?.GridItems[pos];
            bool showASide = item?.GetUpward() ?? CurrentUpside;
            
            if (showASide)
            {
                content.ASide = value;
            }
            else
            {
                content.BSide = value;
            }
            
            BoardDictionary[pos] = content;
        }
        
        /// <summary>
        /// 判断指定位置是否可通行（已消除）
        /// 用于路径搜索算法
        /// </summary>
        /// <param name="pos">格子坐标</param>
        /// <returns>如果已消除（向上数字为0）则返回true</returns>
        private bool IsPositionPassable((int, int) pos)
        {
            return GetUpwardValue(pos) == 0;
        }
        
        #endregion
    }
}