using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.Puzzle
{
    public sealed class BoardAnimator : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private RectTransform tileRoot;
        [SerializeField] private bool logEvents = true;

        private readonly Queue<IEnumerator> queue = new Queue<IEnumerator>();
        private bool isPlaying;
        private bool inputLocked;

        public bool IsInputLocked
        {
            get { return inputLocked; }
        }

        private void Awake()
        {
            if (boardView == null)
            {
                boardView = GetComponent<BoardView>();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnTileSwapped += HandleTileSwapped;
            GameEvents.OnMatchFound += HandleMatchFound;
            GameEvents.OnSpecialTileCreated += HandleSpecialTileCreated;
            GameEvents.OnCascadeStarted += HandleCascadeStarted;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnTileSwapped -= HandleTileSwapped;
            GameEvents.OnMatchFound -= HandleMatchFound;
            GameEvents.OnSpecialTileCreated -= HandleSpecialTileCreated;
            GameEvents.OnCascadeStarted -= HandleCascadeStarted;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            DOTween.Kill(this);
            queue.Clear();
            isPlaying = false;
            inputLocked = false;
        }

        public void Enqueue(IEnumerator animation)
        {
            if (animation == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "BoardAnimator.Enqueue ignored null animation.");
                return;
            }

            queue.Enqueue(animation);

            if (isPlaying == false)
            {
                StartCoroutine(PlayQueue());
            }
        }

        private IEnumerator PlayQueue()
        {
            isPlaying = true;
            inputLocked = true;

            while (queue.Count > 0)
            {
                IEnumerator item = queue.Dequeue();

                if (item != null)
                {
                    yield return StartCoroutine(item);
                }
            }

            inputLocked = false;
            isPlaying = false;
        }

        private void HandleTileSwapped(int x1, int y1, int x2, int y2)
        {
            Enqueue(AnimateSwap(x1, y1, x2, y2, true));
        }

        private void HandleMatchFound(int count)
        {
            Enqueue(AnimateMatchPop(count));
        }

        private void HandleSpecialTileCreated(SpecialItemType type)
        {
            Enqueue(AnimateSpecialCreate(type));
        }

        private void HandleCascadeStarted(int depth)
        {
            Enqueue(AnimateCascadeDelay(depth));
        }

        private void HandleLevelCompleted(int level, int stars)
        {
            Enqueue(AnimateLevelClearStars(stars));
        }

        public IEnumerator AnimateSwap(int x1, int y1, int x2, int y2, bool success)
        {
            if (logEvents == true)
            {
                DebugLogger.Info(LogCategory.Puzzle, $"AnimateSwap {x1},{y1} -> {x2},{y2}, success={success}");
            }

            Transform a = FindTileTransform(x1, y1);
            Transform b = FindTileTransform(x2, y2);

            if (a == null || b == null)
            {
                yield return new WaitForSeconds(success == true ? GameConstants.Timing.TileSwapSeconds : GameConstants.Timing.InvalidSwapReturnSeconds);
                yield break;
            }

            Vector3 aPos = a.localPosition;
            Vector3 bPos = b.localPosition;
            float duration = success == true ? GameConstants.Timing.TileSwapSeconds : GameConstants.Timing.InvalidSwapReturnSeconds;

            Sequence sequence = DOTween.Sequence().SetId(this);
            sequence.Join(a.DOLocalMove(bPos, duration).SetEase(Ease.OutQuad));
            sequence.Join(b.DOLocalMove(aPos, duration).SetEase(Ease.OutQuad));

            if (success == false)
            {
                sequence.Append(a.DOLocalMove(aPos, GameConstants.Timing.InvalidSwapReturnSeconds).SetEase(Ease.OutQuad));
                sequence.Join(b.DOLocalMove(bPos, GameConstants.Timing.InvalidSwapReturnSeconds).SetEase(Ease.OutQuad));
            }

            yield return sequence.WaitForCompletion();
        }

        public IEnumerator AnimateMatchPop(int count)
        {
            int safeCount = Mathf.Max(0, count);

            for (int i = 0; i < safeCount; i++)
            {
                yield return new WaitForSeconds(GameConstants.Timing.MatchPopStaggerSeconds);
            }
        }

        public IEnumerator AnimateGravity(IList<Transform> tiles, IList<Vector3> targetPositions)
        {
            if (tiles == null || targetPositions == null)
            {
                yield break;
            }

            int count = Mathf.Min(tiles.Count, targetPositions.Count);
            Sequence sequence = DOTween.Sequence().SetId(this);

            for (int i = 0; i < count; i++)
            {
                if (tiles[i] == null)
                {
                    continue;
                }

                float duration = Random.Range(GameConstants.Timing.TileDropMinSeconds, GameConstants.Timing.TileDropMaxSeconds);
                sequence.Join(tiles[i].DOLocalMove(targetPositions[i], duration).SetEase(Ease.OutBack));
            }

            yield return sequence.WaitForCompletion();
        }

        public IEnumerator AnimateSpawn(Transform tile)
        {
            if (tile == null)
            {
                yield break;
            }

            tile.localScale = Vector3.zero;
            yield return tile.DOScale(Vector3.one, GameConstants.Timing.TileSpawnSeconds).SetEase(Ease.OutBack).SetId(this).WaitForCompletion();
        }

        public IEnumerator AnimateSpecialCreate(SpecialItemType type)
        {
            yield return new WaitForSeconds(GameConstants.Timing.SpecialCreateSeconds);
        }

        public IEnumerator AnimateCascadeDelay(int depth)
        {
            yield return new WaitForSeconds(GameConstants.Timing.CascadeDelaySeconds);
        }

        public IEnumerator AnimateLevelClearStars(int stars)
        {
            int safeStars = Mathf.Clamp(stars, 0, 3);

            for (int i = 0; i < safeStars; i++)
            {
                yield return new WaitForSeconds(GameConstants.Timing.LevelClearStarPopSeconds);
            }
        }

        private Transform FindTileTransform(int x, int y)
        {
            Transform root = tileRoot != null ? tileRoot : transform;
            string expectedName = $"Tile_{x}_{y}";
            Transform found = root.Find(expectedName);

            if (found != null)
            {
                return found;
            }

            TileView[] tiles = root.GetComponentsInChildren<TileView>(true);

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null)
                {
                    continue;
                }

                if (tiles[i].X == x && tiles[i].Y == y)
                {
                    return tiles[i].transform;
                }
            }

            return null;
        }
    }
}
