using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// Board의 8x8 TileData를 화면에 표시하고 입력을 처리하는 컨트롤러
    /// 첫 클릭 → 선택, 두 번째 인접 클릭 → Board.TrySwapTiles 호출
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public Board board;
        public LevelGoal levelGoal;
        public RectTransform gridContainer;
        public TextMeshProUGUI goalText;
        public TextMeshProUGUI movesText;
        public TextMeshProUGUI statusText;

        private const int GRID_SIZE = GameConstants.Board.Size;
        private TileView[,] views = new TileView[GRID_SIZE, GRID_SIZE];
        private TileView selectedView;

        private bool eventsBound;

        public void BuildGrid()
        {
            if (gridContainer == null)
            {
                DebugLogger.Error(LogCategory.UI, "[BoardView] gridContainer is not assigned", this);
                return;
            }

            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    GameObject go = new GameObject(
                        $"Tile_{x}_{y}",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(TileView)
                    );
                    go.transform.SetParent(gridContainer, false);

                    TileView tv = go.GetComponent<TileView>();
                    if (tv == null)
                    {
                        DebugLogger.Error(LogCategory.UI, $"[BoardView] TileView component missing on Tile_{x}_{y}", this);
                        Destroy(go);
                        continue;
                    }

                    tv.Setup(x, y, this);
                    SetViewSafe(x, y, tv);
                }
            }

            BindBoardEvents();
            RefreshAll();
            UpdateStatus("");
        }

        private void BindBoardEvents()
        {
            if (eventsBound)
            {
                return;
            }
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[BoardView] Cannot bind events because board is null", this);
                return;
            }
            board.OnLevelComplete += HandleLevelComplete;
            board.OnLevelFailed += HandleLevelFailed;
            eventsBound = true;
        }

        private void OnDestroy()
        {
            if (board != null && eventsBound)
            {
                board.OnLevelComplete -= HandleLevelComplete;
                board.OnLevelFailed -= HandleLevelFailed;
                eventsBound = false;
            }
        }

        public void OnTileClicked(TileView clicked)
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[BoardView] Tile click ignored because board is null", this);
                return;
            }
            if (clicked == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[BoardView] Tile click ignored because clicked view is null", this);
                return;
            }
            if (!IsValidPosition(clicked.x, clicked.y))
            {
                DebugLogger.Warning(LogCategory.UI, $"[BoardView] Tile click ignored due to invalid coordinates: ({clicked.x},{clicked.y})", this);
                return;
            }
            if (board.IsLevelComplete())
            {
                return;
            }
            if (levelGoal != null && levelGoal.IsMovesExceeded())
            {
                return;
            }

            if (selectedView == null)
            {
                selectedView = clicked;
                clicked.SetSelected(true);
                UpdateStatus("");
                return;
            }

            if (selectedView == clicked)
            {
                clicked.SetSelected(false);
                selectedView = null;
                return;
            }

            int dx = Mathf.Abs(selectedView.x - clicked.x);
            int dy = Mathf.Abs(selectedView.y - clicked.y);
            bool adjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

            if (adjacent)
            {
                int sx = selectedView.x, sy = selectedView.y;
                bool ok = board.TrySwapTiles(sx, sy, clicked.x, clicked.y);
                // selectedView could have been destroyed by RefreshAll / re-spawn pipeline below;
                // null-check before SetSelected to guard against rare race after TrySwapTiles.
                if (selectedView != null)
                {
                    selectedView.SetSelected(false);
                }
                selectedView = null;
                RefreshAll();

                if (!ok)
                {
                    UpdateStatus("매치 없음 — 스왑 취소");
                }
                else
                {
                    UpdateStatus("");
                }
            }
            else
            {
                if (selectedView != null)
                {
                    selectedView.SetSelected(false);
                }
                selectedView = clicked;
                if (clicked != null)
                {
                    clicked.SetSelected(true);
                }
            }
        }

        public void RefreshAll()
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[BoardView] RefreshAll ignored because board is null", this);
                return;
            }

            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    TileData td = board.GetTile(x, y);
                    TileView view = GetViewSafe(x, y);
                    if (view != null)
                    {
                        view.Refresh(td);
                    }
                }
            }

            if (levelGoal != null)
            {
                if (goalText != null)
                {
                    goalText.text = $"{levelGoal.GetGoalDescription()}  ({levelGoal.GetProgressDescription()})";
                }
                if (movesText != null)
                {
                    movesText.text = $"이동 {levelGoal.movesUsed}/{levelGoal.moveLimit}";
                }
            }
        }

        private void HandleLevelComplete(int stars)
        {
            string starStr = new string('★', Mathf.Clamp(stars, 0, 3));
            UpdateStatus($"클리어! {starStr}");
            if (selectedView != null)
            {
                selectedView.SetSelected(false);
                selectedView = null;
            }
            RefreshAll();
        }

        private void HandleLevelFailed()
        {
            UpdateStatus("이동 부족 — 실패");
            if (selectedView != null)
            {
                selectedView.SetSelected(false);
                selectedView = null;
            }
            RefreshAll();
        }

        private void UpdateStatus(string msg)
        {
            if (statusText != null)
            {
                statusText.text = msg;
            }
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GRID_SIZE && y >= 0 && y < GRID_SIZE;
        }

        private TileView GetViewSafe(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                DebugLogger.Warning(LogCategory.UI, $"[BoardView] Out-of-bounds view access: ({x},{y})", this);
                return null;
            }
            if (views == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[BoardView] views array is null", this);
                return null;
            }
            return views[y, x];
        }

        private void SetViewSafe(int x, int y, TileView view)
        {
            if (!IsValidPosition(x, y))
            {
                DebugLogger.Warning(LogCategory.UI, $"[BoardView] Ignored SetViewSafe out-of-bounds: ({x},{y})", this);
                return;
            }
            if (views == null)
            {
                views = new TileView[GRID_SIZE, GRID_SIZE];
            }
            views[y, x] = view;
        }
    }
}
