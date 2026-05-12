using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    public sealed class TileView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerUpHandler
    {
        [Header("Runtime References")]
        [SerializeField] private Board board;
        [SerializeField] private BoardView boardView;

        [Header("Tile Position")]
        public int x;
        public int y;

        [Header("Drag Settings")]
        [SerializeField] private float dragThresholdPixels = 42f;

        [Header("Optional Visuals")]
        [SerializeField] private Image tileImage;
        [SerializeField] private RectTransform rectTransform;

        // Backward-compat visuals — owned by Setup/Refresh/SetSelected (BoardView path).
        private Image image;
        private Outline outline;

        // ===== Static sprite/color tables (preserved API; consumed by AppBootstrap & TileSpriteBinder) =====

        // TileType 인덱스 → 색상 (sprite 미지정 시 fallback)
        private static readonly Color[] TileColors = new Color[]
        {
            new Color(0.30f, 0.55f, 0.95f, 1f), // Fish     - 파랑
            new Color(0.95f, 0.95f, 0.92f, 1f), // Milk     - 흰
            new Color(1.00f, 0.55f, 0.78f, 1f), // Yarn     - 분홍
            new Color(0.45f, 0.80f, 0.40f, 1f), // Catnip   - 녹색
            new Color(1.00f, 0.85f, 0.30f, 1f), // Pawprint - 노랑
            new Color(0.65f, 0.45f, 0.85f, 1f), // Fishbone - 보라
        };

        private static readonly Color EmptyColor = new Color(0.15f, 0.15f, 0.18f, 0.4f);

        private static Sprite[] s_tileSprites;

        public static void SetTileSprites(Sprite[] sprites)
        {
            s_tileSprites = sprites;
        }

        private static Sprite cachedWhiteSprite;

        public static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return cachedWhiteSprite;
        }

        // ===== Drag-to-swap state (Phase 3) =====
        private static TileView selectedTile;

        private Vector2 dragStartScreenPosition;
        private bool isDragging;
        private bool swapTriggeredDuringDrag;
        private bool pointerUpConsumedByDrag;

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        // ===== Phase 3 Initialize overloads =====

        public void Initialize(Board targetBoard, int gridX, int gridY)
        {
            board = targetBoard;
            x = gridX;
            y = gridY;

            CacheComponents();
        }

        public void Initialize(BoardView targetBoardView, Board targetBoard, int gridX, int gridY)
        {
            boardView = targetBoardView;
            board = targetBoard;
            x = gridX;
            y = gridY;

            CacheComponents();
        }

        public void SetBoard(Board targetBoard)
        {
            board = targetBoard;
        }

        public void SetBoardView(BoardView targetBoardView)
        {
            boardView = targetBoardView;
        }

        public void SetCoordinates(int gridX, int gridY)
        {
            x = gridX;
            y = gridY;
        }

        public void SetPosition(int gridX, int gridY)
        {
            x = gridX;
            y = gridY;
        }

        public void RefreshCoordinates(int gridX, int gridY)
        {
            x = gridX;
            y = gridY;
        }

        // ===== BoardView-compat API (preserved from previous TileView) =====

        public void Setup(int posX, int posY, BoardView parent)
        {
            x = posX;
            y = posY;
            boardView = parent;

            // Phase 3 drag-to-swap needs a Board reference. Derive it from BoardView.
            if (parent != null && parent.board != null)
            {
                board = parent.board;
            }

            CacheComponents();

            if (image == null)
            {
                image = tileImage != null ? tileImage : GetComponent<Image>();
            }
            if (image != null)
            {
                image.sprite = GetWhiteSprite();
                image.raycastTarget = true;
                image.color = EmptyColor;
            }

            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.95f, 0.25f, 1f);
                outline.effectDistance = new Vector2(4, -4);
                outline.enabled = false;
            }
        }

        public void Refresh(TileData data)
        {
            if (image == null)
            {
                image = tileImage != null ? tileImage : GetComponent<Image>();
            }
            if (image == null)
            {
                return;
            }

            if (data == null)
            {
                image.sprite = GetWhiteSprite();
                Debug.Log($"[TILE] {gameObject.name} sprite={image.sprite?.name ?? "NULL"} color={image.color}");
                image.color = EmptyColor;
                return;
            }

            x = data.x;
            y = data.y;

            int idx = (int)data.type;

            // sprite 주입돼 있으면 sprite로 렌더, color는 흰색(틴팅 없음)
            if (s_tileSprites != null && idx >= 0 && idx < s_tileSprites.Length && s_tileSprites[idx] != null)
            {
                image.sprite = s_tileSprites[idx];
                Debug.Log($"[TILE] {gameObject.name} sprite={image.sprite?.name ?? "NULL"} color={image.color}");
                image.color = Color.white;
                return;
            }

            // 미주입 fallback: 흰 sprite + 타일별 색상
            image.sprite = GetWhiteSprite();
            Debug.Log($"[TILE] {gameObject.name} sprite={image.sprite?.name ?? "NULL"} color={image.color}");
            if (idx >= 0 && idx < TileColors.Length)
            {
                image.color = TileColors[idx];
            }
            else
            {
                image.color = EmptyColor;
            }
        }

        public void SetSelected(bool selected)
        {
            if (outline != null)
            {
                outline.enabled = selected;
            }
        }

        // ===== Lifecycle =====

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();

                if (rectTransform == null)
                {
                    DebugLogger.Warning(LogCategory.Puzzle, "TileView missing RectTransform.");
                }
            }

            if (tileImage == null)
            {
                tileImage = GetComponent<Image>();

                if (tileImage == null)
                {
                    tileImage = GetComponentInChildren<Image>();

                    if (tileImage == null)
                    {
                        DebugLogger.Warning(LogCategory.Puzzle, "TileView missing Image component.");
                    }
                }
            }

            if (image == null)
            {
                image = tileImage;
            }
        }

        // ===== Pointer & drag handlers =====

        public void OnPointerClick(PointerEventData eventData)
        {
            if (pointerUpConsumedByDrag == true)
            {
                pointerUpConsumedByDrag = false;
                return;
            }

            if (eventData == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TileView.OnPointerClick received null eventData.");
                return;
            }

            // Legacy click-to-click: when a BoardView is wired up, let it own selection.
            // Drag-to-swap below still works regardless of this routing.
            if (boardView != null)
            {
                boardView.OnTileClicked(this);
                return;
            }

            HandleClickFallback();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TileView.OnBeginDrag received null eventData.");
                return;
            }

            dragStartScreenPosition = eventData.position;
            isDragging = true;
            swapTriggeredDuringDrag = false;
            pointerUpConsumedByDrag = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging == false)
            {
                return;
            }

            if (swapTriggeredDuringDrag == true)
            {
                return;
            }

            if (eventData == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TileView.OnDrag received null eventData.");
                return;
            }

            Vector2 dragDelta = eventData.position - dragStartScreenPosition;

            if (dragDelta.magnitude < dragThresholdPixels)
            {
                return;
            }

            Vector2Int direction = GetDominantDirection(dragDelta);

            if (direction == Vector2Int.zero)
            {
                return;
            }

            TrySwapByDirection(direction);
            swapTriggeredDuringDrag = true;
            pointerUpConsumedByDrag = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (swapTriggeredDuringDrag == true)
            {
                pointerUpConsumedByDrag = true;
            }

            isDragging = false;
            swapTriggeredDuringDrag = false;
        }

        private void HandleClickFallback()
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TileView click ignored because Board is null.");
                return;
            }

            if (selectedTile == null)
            {
                selectedTile = this;
                SetSelectedVisual(true);
                return;
            }

            if (selectedTile == this)
            {
                selectedTile.SetSelectedVisual(false);
                selectedTile = null;
                return;
            }

            if (AreAdjacent(selectedTile, this) == false)
            {
                selectedTile.SetSelectedVisual(false);
                selectedTile = this;
                SetSelectedVisual(true);
                return;
            }

            int fromX = selectedTile.X;
            int fromY = selectedTile.Y;
            int toX = X;
            int toY = Y;

            selectedTile.SetSelectedVisual(false);
            selectedTile = null;

            TrySwap(fromX, fromY, toX, toY);
        }

        private void TrySwapByDirection(Vector2Int direction)
        {
            int targetX = x + direction.x;
            int targetY = y + direction.y;

            if (IsInsideBoard(targetX, targetY) == false)
            {
                DebugLogger.Info(LogCategory.Puzzle, $"Drag swap ignored. Target out of board: ({targetX}, {targetY})");
                return;
            }

            TrySwap(x, y, targetX, targetY);
        }

        private void TrySwap(int fromX, int fromY, int toX, int toY)
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TrySwap failed because Board is null.");
                return;
            }

            if (IsInsideBoard(fromX, fromY) == false)
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"TrySwap from position out of bounds: ({fromX}, {fromY})");
                return;
            }

            if (IsInsideBoard(toX, toY) == false)
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"TrySwap to position out of bounds: ({toX}, {toY})");
                return;
            }

            bool success = board.TrySwapTiles(fromX, fromY, toX, toY);

            if (success == true)
            {
                // Board / BoardAdapter raise GameEvents.RaiseTileSwapped internally — don't double-fire.
                DebugLogger.Info(LogCategory.Puzzle, $"Tile swap accepted: ({fromX},{fromY}) -> ({toX},{toY})");

                // Drag path bypasses BoardView.OnTileClicked, so trigger redraw manually when wired.
                if (boardView != null)
                {
                    boardView.RefreshAll();
                }
            }
            else
            {
                DebugLogger.Info(LogCategory.Puzzle, $"Tile swap rejected: ({fromX},{fromY}) -> ({toX},{toY})");
            }
        }

        private Vector2Int GetDominantDirection(Vector2 dragDelta)
        {
            if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
            {
                if (dragDelta.x > 0f)
                {
                    return Vector2Int.right;
                }

                return Vector2Int.left;
            }

            if (dragDelta.y > 0f)
            {
                return Vector2Int.up;
            }

            return Vector2Int.down;
        }

        private bool AreAdjacent(TileView a, TileView b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            int dx = Mathf.Abs(a.X - b.X);
            int dy = Mathf.Abs(a.Y - b.Y);

            return dx + dy == 1;
        }

        private bool IsInsideBoard(int gridX, int gridY)
        {
            return gridX >= 0 &&
                   gridX < GameConstants.Board.Size &&
                   gridY >= 0 &&
                   gridY < GameConstants.Board.Size;
        }

        private void SetSelectedVisual(bool selected)
        {
            if (tileImage == null)
            {
                return;
            }

            if (selected == true)
            {
                tileImage.transform.localScale = Vector3.one * 1.08f;
            }
            else
            {
                tileImage.transform.localScale = Vector3.one;
            }
        }

        private void OnDisable()
        {
            if (selectedTile == this)
            {
                selectedTile = null;
            }

            isDragging = false;
            swapTriggeredDuringDrag = false;
            pointerUpConsumedByDrag = false;
        }

        private void OnDestroy()
        {
            if (selectedTile == this)
            {
                selectedTile = null;
            }
        }
    }
}
