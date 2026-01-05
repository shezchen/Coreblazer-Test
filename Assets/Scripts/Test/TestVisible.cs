using System.Linq;
using Architecture;
using GamePlay;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Test
{
    public class TestVisible : SerializedMonoBehaviour
    {
        [Inject, ShowInInspector, ReadOnly] 
        private GamePlayManager _gamePlayManager;

        [Title("Controls")]
        [BoxGroup("Controls")]
        [Button("Initialize Board", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        [PropertyTooltip("Call ResetManager() to generate a new 10x10 board")]
        private void InitializeBoard()
        {
            if (_gamePlayManager == null)
            {
                Debug.LogError("GamePlayManager is not injected!");
                return;
            }
            
            _gamePlayManager.ResetManager();
            RefreshDisplay();
            Debug.Log("Board initialized successfully!");
        }

        [BoxGroup("Controls")]
        [Button("Refresh Display", ButtonSizes.Medium), GUIColor(0.8f, 1f, 0.4f)]
        [PropertyTooltip("Update the visual display from current BoardDictionary data")]
        private void RefreshDisplay()
        {
            if (_gamePlayManager?.BoardDictionary == null)
            {
                _boardDisplay = null;
                return;
            }
            
            _boardDisplay = new string[10, 10];
            
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    // Invert row to make (0,0) appear at bottom-left instead of top-left
                    int displayRow = 9 - row;
                    
                    if (_gamePlayManager.BoardDictionary.TryGetValue((row, col), out GridContent content))
                    {
                        _boardDisplay[displayRow, col] = $"A:{content.ASide} B:{content.BSide}";
                    }
                    else
                    {
                        _boardDisplay[displayRow, col] = "Empty";
                    }
                }
            }
        }

        [Title("Board Data (10x10 Grid)")]
        [ShowInInspector, ReadOnly]
        [TableMatrix(SquareCells = true, HideColumnIndices = true, HideRowIndices = false)]
        [PropertyTooltip("Visual representation of the 10x10 board. Each cell shows ASide (even) and BSide (odd) values.")]
        private string[,] BoardDisplay
        {
            get
            {
                if (_gamePlayManager?.BoardDictionary == null || _gamePlayManager.BoardDictionary.Count == 0)
                {
                    return CreateEmptyBoard();
                }
                
                if (_boardDisplay == null)
                {
                    RefreshDisplay();
                }
                
                return _boardDisplay ?? CreateEmptyBoard();
            }
        }

        private string[,] _boardDisplay;

        [Title("Statistics")]
        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Total number of cells in BoardDictionary")]
        private int CellCount => _gamePlayManager?.BoardDictionary?.Count ?? 0;

        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Number of pairs that sum to 11 (should be 0)")]
        private int InvalidPairsCount
        {
            get
            {
                if (_gamePlayManager?.BoardDictionary == null) return 0;
                return _gamePlayManager.BoardDictionary.Values.Count(g => g.ASide + g.BSide == 11);
            }
        }

        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Verification that all ASide values are even")]
        private bool AllASidesEven
        {
            get
            {
                if (_gamePlayManager?.BoardDictionary == null) return false;
                return _gamePlayManager.BoardDictionary.Values.All(g => g.ASide % 2 == 0);
            }
        }

        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Verification that all BSide values are odd")]
        private bool AllBSidesOdd
        {
            get
            {
                if (_gamePlayManager?.BoardDictionary == null) return false;
                return _gamePlayManager.BoardDictionary.Values.All(g => g.BSide % 2 == 1);
            }
        }

        private void Awake()
        {
            ScopeRef.LifetimeScope.Container.Inject(this);
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _gamePlayManager?.BoardDictionary != null && _gamePlayManager.BoardDictionary.Count > 0)
            {
                RefreshDisplay();
            }
        }

        private string[,] CreateEmptyBoard()
        {
            var emptyBoard = new string[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    emptyBoard[i, j] = "---";
                }
            }
            return emptyBoard;
        }
    }
}