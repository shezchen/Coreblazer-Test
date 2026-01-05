using System;
using System.Collections.Generic;

namespace Tools
{
    /// <summary>
    /// 连连看路径搜索工具类
    /// 使用 BFS 算法在 12x12 扩展网格中查找有效路径
    /// </summary>
    public static class LinkPathFinder
    {
        /// <summary>
        /// 原始棋盘尺寸
        /// </summary>
        private const int BoardSize = 10;
        
        /// <summary>
        /// 扩展后的网格最小索引（外圈）
        /// </summary>
        private const int MinIndex = -1;
        
        /// <summary>
        /// 扩展后的网格最大索引（外圈）
        /// </summary>
        private const int MaxIndex = 10;
        
        /// <summary>
        /// 四个方向的偏移量（上、下、左、右）
        /// </summary>
        private static readonly (int dRow, int dCol)[] Directions = 
        {
            (-1, 0),  // 上
            (1, 0),   // 下
            (0, -1),  // 左
            (0, 1)    // 右
        };
        
        /// <summary>
        /// 判断两个位置之间是否存在有效路径
        /// </summary>
        /// <param name="start">起点坐标（0-9范围内的棋盘坐标）</param>
        /// <param name="end">终点坐标（0-9范围内的棋盘坐标）</param>
        /// <param name="isPassable">判断某格子是否可通行的委托（返回true表示该格子已消除可通过）</param>
        /// <returns>是否存在有效路径</returns>
        public static bool HasValidPath((int row, int col) start, (int row, int col) end, Func<(int, int), bool> isPassable)
        {
            // 如果起点和终点相同，无效
            if (start == end)
            {
                return false;
            }
            
            // BFS 搜索
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int row, int col)>();
            
            // 从起点开始，起点本身不需要可通行（它是被选中的格子）
            queue.Enqueue(start);
            visited.Add(start);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                // 遍历四个方向
                foreach (var (dRow, dCol) in Directions)
                {
                    int newRow = current.row + dRow;
                    int newCol = current.col + dCol;
                    var newPos = (newRow, newCol);
                    
                    // 检查是否已访问
                    if (visited.Contains(newPos))
                    {
                        continue;
                    }
                    
                    // 检查是否到达终点
                    if (newPos == end)
                    {
                        return true;
                    }
                    
                    // 检查是否可通行
                    if (IsPositionPassable(newPos, isPassable))
                    {
                        visited.Add(newPos);
                        queue.Enqueue(newPos);
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 判断某个位置是否可通行
        /// </summary>
        /// <param name="pos">位置坐标</param>
        /// <param name="isPassable">判断棋盘内格子是否可通行的委托</param>
        /// <returns>是否可通行</returns>
        private static bool IsPositionPassable((int row, int col) pos, Func<(int, int), bool> isPassable)
        {
            // 超出扩展网格范围，不可通行
            if (pos.row < MinIndex || pos.row > MaxIndex || pos.col < MinIndex || pos.col > MaxIndex)
            {
                return false;
            }
            
            // 外圈（扩展的边界）始终可通行
            if (pos.row == MinIndex || pos.row == MaxIndex || pos.col == MinIndex || pos.col == MaxIndex)
            {
                return true;
            }
            
            // 棋盘内的格子，使用委托判断是否已消除
            return isPassable(pos);
        }
        
        /// <summary>
        /// 获取有效路径（用于后期绘制路径动画）
        /// </summary>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="isPassable">判断某格子是否可通行的委托</param>
        /// <returns>路径坐标列表，如果没有有效路径则返回null</returns>
        public static List<(int row, int col)> FindPath((int row, int col) start, (int row, int col) end, Func<(int, int), bool> isPassable)
        {
            if (start == end)
            {
                return null;
            }
            
            // BFS 搜索，记录父节点用于回溯路径
            var visited = new HashSet<(int, int)>();
            var parent = new Dictionary<(int, int), (int, int)>();
            var queue = new Queue<(int row, int col)>();
            
            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = start; // 起点的父节点是自己
            
            bool found = false;
            
            while (queue.Count > 0 && !found)
            {
                var current = queue.Dequeue();
                
                foreach (var (dRow, dCol) in Directions)
                {
                    int newRow = current.row + dRow;
                    int newCol = current.col + dCol;
                    var newPos = (newRow, newCol);
                    
                    if (visited.Contains(newPos))
                    {
                        continue;
                    }
                    
                    if (newPos == end)
                    {
                        parent[newPos] = current;
                        found = true;
                        break;
                    }
                    
                    if (IsPositionPassable(newPos, isPassable))
                    {
                        visited.Add(newPos);
                        parent[newPos] = current;
                        queue.Enqueue(newPos);
                    }
                }
            }
            
            if (!found)
            {
                return null;
            }
            
            // 回溯路径
            var path = new List<(int row, int col)>();
            var node = end;
            while (node != start)
            {
                path.Add(node);
                node = parent[node];
            }
            path.Add(start);
            path.Reverse();
            
            return path;
        }
    }
}

