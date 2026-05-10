using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    public sealed class HintSystem : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private BoardView boardView;
        [SerializeField] private Image highlightRing;
        [SerializeField] private RectTransform boardRect;
        [SerializeField] private float idleSeconds = 0f;

        private Coroutine idleRoutine;
        private Vector2Int currentHintA;
        private Vector2Int currentHintB;

        private void Awake()
        {
            if (board == null)
            {
                board = FindObjectOfType<Board>();
            }

            if (boardView == null)
            {
                boardView = FindObjectOfType<BoardView>();
            }

            if (highlightRing != null)
            {
                highlightRing.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnTileSwapped += HandleInput;
            RestartIdleTimer();
        }

        private void OnDisable()
        {
            GameEvents.OnTileSwapped -= HandleInput;
            StopIdleTimer();
            HideHint();
        }

        public void NotifyPlayerInput()
        {
            HideHint();
            RestartIdleTimer();
        }

        private void HandleInput(int x1, int y1, int x2, int y2)
        {
            NotifyPlayerInput();
        }

        private void RestartIdleTimer()
        {
            StopIdleTimer();

            if (isActiveAndEnabled == true)
            {
                idleRoutine = StartCoroutine(IdleTimer());
            }
        }

        private void StopIdleTimer()
        {
            if (idleRoutine != null)
            {
                StopCoroutine(idleRoutine);
                idleRoutine = null;
            }
        }

        private IEnumerator IdleTimer()
        {
            float delay = idleSeconds > 0f ? idleSeconds : GameConstants.Timing.HintIdleSeconds;
            yield return new WaitForSeconds(delay);
            ShowBestHintOrShuffle();
        }

        public void ShowBestHintOrShuffle()
        {
            List<HintCandidate> candidates = FindValidSwaps();

            if (candidates.Count == 0)
            {
                ShuffleBoardUntilValid();
                candidates = FindValidSwaps();
            }

            if (candidates.Count == 0)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "HintSystem could not find valid swap after shuffle.");
                return;
            }

            candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
            currentHintA = candidates[0].A;
            currentHintB = candidates[0].B;
            ShowHintAt(currentHintA, currentHintB);
        }

        private List<HintCandidate> FindValidSwaps()
        {
            List<HintCandidate> result = new List<HintCandidate>();

            for (int x = 0; x < GameConstants.Board.Size; x++)
            {
                for (int y = 0; y < GameConstants.Board.Size; y++)
                {
                    EvaluateCandidate(x, y, x + 1, y, result);
                    EvaluateCandidate(x, y, x, y + 1, result);
                }
            }

            return result;
        }

        private void EvaluateCandidate(int x1, int y1, int x2, int y2, List<HintCandidate> result)
        {
            if (result == null)
            {
                return;
            }

            if (IsInside(x1, y1) == false || IsInside(x2, y2) == false)
            {
                return;
            }

            int score = EstimateSwapScore(x1, y1, x2, y2);

            if (score > 0)
            {
                result.Add(new HintCandidate(new Vector2Int(x1, y1), new Vector2Int(x2, y2), score));
            }
        }

        private int EstimateSwapScore(int x1, int y1, int x2, int y2)
        {
            if (board == null)
            {
                return 0;
            }

            int t1 = board.GetTileType(x1, y1);
            int t2 = board.GetTileType(x2, y2);

            if (t1 < 0 || t2 < 0 || t1 == t2)
            {
                return 0;
            }

            int score = CountPotentialAt(x1, y1, t2) + CountPotentialAt(x2, y2, t1);
            return score >= 3 ? score : 0;
        }

        private int CountPotentialAt(int x, int y, int type)
        {
            int horizontal = 1;
            horizontal += CountDirection(x, y, type, 1, 0);
            horizontal += CountDirection(x, y, type, -1, 0);

            int vertical = 1;
            vertical += CountDirection(x, y, type, 0, 1);
            vertical += CountDirection(x, y, type, 0, -1);

            return Mathf.Max(horizontal, vertical);
        }

        private int CountDirection(int x, int y, int type, int dx, int dy)
        {
            int count = 0;
            int cx = x + dx;
            int cy = y + dy;

            while (IsInside(cx, cy) == true && board != null && board.GetTileType(cx, cy) == type)
            {
                count++;
                cx += dx;
                cy += dy;
            }

            return count;
        }

        private void ShuffleBoardUntilValid()
        {
            if (board == null)
            {
                return;
            }

            for (int i = 0; i < 20; i++)
            {
                board.ShuffleBoard();

                if (FindValidSwaps().Count > 0)
                {
                    if (boardView != null)
                    {
                        boardView.RefreshAll();
                    }

                    return;
                }
            }
        }

        private void ShowHintAt(Vector2Int a, Vector2Int b)
        {
            if (highlightRing == null)
            {
                return;
            }

            highlightRing.gameObject.SetActive(true);
            RectTransform rt = highlightRing.rectTransform;
            Vector2 middle = Vector2.Lerp(a, b, 0.5f);
            rt.anchoredPosition = GridToAnchoredPosition(middle);
        }

        private void HideHint()
        {
            if (highlightRing != null)
            {
                highlightRing.gameObject.SetActive(false);
            }
        }

        private Vector2 GridToAnchoredPosition(Vector2 grid)
        {
            float cell = GameConstants.Board.CellSize;
            float half = (GameConstants.Board.Size - 1) * cell * 0.5f;
            return new Vector2(grid.x * cell - half, half - grid.y * cell);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < GameConstants.Board.Size && y >= 0 && y < GameConstants.Board.Size;
        }

        private readonly struct HintCandidate
        {
            public readonly Vector2Int A;
            public readonly Vector2Int B;
            public readonly int Score;

            public HintCandidate(Vector2Int a, Vector2Int b, int score)
            {
                A = a;
                B = b;
                Score = score;
            }
        }
    }
}
