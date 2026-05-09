using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private const int GRID_SIZE = 8;
        private TileView[,] views = new TileView[GRID_SIZE, GRID_SIZE];
        private TileView selectedView;

        private bool eventsBound;

        public void BuildGrid()
        {
            if (gridContainer == null)
            {
                Debug.LogError("[BoardView] gridContainer is not assigned");
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
                    tv.Setup(x, y, this);
                    views[y, x] = tv;
                }
            }

            BindBoardEvents();
            RefreshAll();
            UpdateStatus("");
        }

        private void BindBoardEvents()
        {
            if (board == null || eventsBound) return;
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
            if (board == null || clicked == null) return;
            if (board.IsLevelComplete()) return;
            if (levelGoal != null && levelGoal.IsMovesExceeded()) return;

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
                if (selectedView != null) selectedView.SetSelected(false);
                selectedView = null;
                RefreshAll();

                if (!ok)
                    UpdateStatus("매치 없음 — 스왑 취소");
                else
                    UpdateStatus("");
            }
            else
            {
                if (selectedView != null) selectedView.SetSelected(false);
                selectedView = clicked;
                if (clicked != null) clicked.SetSelected(true);
            }
        }

        public void RefreshAll()
        {
            if (board == null) return;

            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    TileData td = board.GetTile(x, y);
                    if (views[y, x] != null) views[y, x].Refresh(td);
                }
            }

            if (levelGoal != null)
            {
                if (goalText != null)
                    goalText.text = $"{levelGoal.GetGoalDescription()}  ({levelGoal.GetProgressDescription()})";
                if (movesText != null)
                    movesText.text = $"이동 {levelGoal.movesUsed}/{levelGoal.moveLimit}";
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
            if (statusText != null) statusText.text = msg;
        }
    }
}
