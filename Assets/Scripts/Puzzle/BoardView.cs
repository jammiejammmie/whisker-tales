using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// Board의 8x8 TileData를 화면에 표시하고 입력을 처리하는 컨트롤러.
    /// 입력 모델: 누름(OnTilePressed) → 드래그 → 손 뗌(OnTileReleased) → 카디널 방향으로 스왑.
    /// 드래그 거리가 타일 크기 30% 미만이면 오터치 방지로 무시. 보드 밖/매치 불가면 bounce 애니메이션.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public Board board;
        public LevelGoal levelGoal;
        public RectTransform gridContainer;
        public TextMeshProUGUI goalText;
        public TextMeshProUGUI movesText;
        public TextMeshProUGUI statusText;

        // 드래그 거리 임계값 (타일 크기 대비 비율). 이 값 미만이면 오터치로 간주.
        public const float DRAG_THRESHOLD_RATIO = 0.3f;

        // Bounce 애니메이션 길이/거리.
        public const float BOUNCE_DURATION = 0.18f;
        public const float BOUNCE_DISTANCE = 18f;

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

        // ===== 입력 콜백 (TileView가 호출) =====

        public void OnTilePressed(TileView pressed)
        {
            if (board == null || pressed == null) return;
            if (board.IsLevelComplete()) return;
            if (levelGoal != null && levelGoal.IsMovesExceeded()) return;

            if (selectedView != null && selectedView != pressed)
            {
                selectedView.SetSelected(false);
            }
            selectedView = pressed;
            pressed.SetSelected(true);
            UpdateStatus("");
        }

        /// <summary>
        /// 드래그 종료 시 호출. dragDelta는 화면 픽셀 단위(시작점 기준 변위).
        /// 임계값 통과 시 카디널 방향 인접 타일과 스왑 시도.
        /// </summary>
        public void OnTileReleased(TileView released, Vector2 dragDelta)
        {
            if (board == null || released == null) return;
            // selectedView 정합성 — Pressed→Released 정상 흐름이면 같지만, 재진입 가드만.
            if (selectedView != released)
            {
                if (selectedView != null) selectedView.SetSelected(false);
                selectedView = null;
            }

            // 임계값: 타일 크기 × 비율
            RectTransform rt = released.transform as RectTransform;
            float tileSize = (rt != null) ? Mathf.Min(rt.rect.width, rt.rect.height) : 100f;
            if (tileSize <= 0f) tileSize = 100f;
            float threshold = tileSize * DRAG_THRESHOLD_RATIO;

            if (dragDelta.magnitude < threshold)
            {
                // 오터치 방지 — 선택만 해제하고 끝
                released.SetSelected(false);
                selectedView = null;
                return;
            }

            // 카디널 방향 결정 — 더 큰 축이 우세
            int dx = 0, dy = 0;
            if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                dx = (dragDelta.x > 0f) ? 1 : -1;
            else
                dy = (dragDelta.y > 0f) ? -1 : 1;
            // 주의: UI Y축은 위로 +지만 grid Y축은 아래로 +. 보드의 (x, y) 좌표계에 맞추기 위해
            // 화면 위쪽 드래그(dragDelta.y > 0)가 grid Y -1(=위쪽 행)이 되도록 부호 반전.

            int targetX = released.x + dx;
            int targetY = released.y + dy;

            // 보드 밖이면 bounce
            if (targetX < 0 || targetX >= GRID_SIZE || targetY < 0 || targetY >= GRID_SIZE)
            {
                if (Application.isPlaying) StartCoroutine(BounceAnimation(released, dx, -dy));
                released.SetSelected(false);
                selectedView = null;
                UpdateStatus("");
                return;
            }

            // 스왑 시도
            int sx = released.x, sy = released.y;
            bool ok = board.TrySwapTiles(sx, sy, targetX, targetY);
            released.SetSelected(false);
            selectedView = null;
            RefreshAll();

            if (!ok)
            {
                // RefreshAll 후에도 released는 같은 위치(스왑 실패 → 보드 변동 없음). bounce 가능.
                if (Application.isPlaying) StartCoroutine(BounceAnimation(released, dx, -dy));
                UpdateStatus("매치 없음 — 스왑 취소");
            }
            else
            {
                UpdateStatus("");
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

        /// <summary>
        /// 스왑 불가 시 살짝 튕기는 애니메이션 — UI 좌표계(위로 +)에 맞춰 dx/dyUI를 받음.
        /// </summary>
        private IEnumerator BounceAnimation(TileView tile, int dx, int dyUI)
        {
            if (tile == null) yield break;
            RectTransform rt = tile.transform as RectTransform;
            if (rt == null) yield break;

            Vector2 origin = rt.anchoredPosition;
            Vector2 bounceOffset = new Vector2(dx * BOUNCE_DISTANCE, dyUI * BOUNCE_DISTANCE);

            float t = 0f;
            while (t < BOUNCE_DURATION)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / BOUNCE_DURATION);
                // 사인 곡선 한 번 — 0 → 1 → 0
                float curve = Mathf.Sin(p * Mathf.PI);
                if (rt != null) rt.anchoredPosition = origin + bounceOffset * curve;
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = origin;
        }
    }
}
