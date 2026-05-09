using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 단일 타일 1개를 화면에 표시하는 UI 컴포넌트.
    /// 입력 방식: 누름 → 드래그(상/하/좌/우) → 손 뗌 → BoardView가 스왑 시도.
    /// 드래그 중 손가락 이동량의 50%만큼 시각적 피드백.
    /// 핵심: 손 뗀 위치가 다른 타일이어도 OnEndDrag는 원래 press 타일에 fire되므로
    /// release 처리는 OnPointerUp이 아니라 OnEndDrag에서 한다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class TileView : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int x;
        public int y;

        // 드래그 시각 피드백 강도. 1.0이면 손가락 따라 1:1, 0.5면 절반만 따라감.
        public const float DRAG_VISUAL_RATIO = 0.5f;

        // 스왑 실패 시 원래 위치로 복귀하는 애니메이션 길이.
        public const float SNAP_BACK_DURATION = 0.18f;

        private Image image;
        private Outline outline;
        private BoardView boardView;
        private RectTransform rt;

        private Vector2 pressPos;
        private Vector2 originalAnchoredPos;
        private bool isPressed;
        private bool dragInitiated;
        private Coroutine snapCoroutine;

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

        // 모든 TileView 인스턴스가 공유하는 sprite 테이블. TileType 순서와 1:1 대응.
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

        public Vector2 OriginAnchoredPos => originalAnchoredPos;

        public void Setup(int posX, int posY, BoardView parent)
        {
            x = posX;
            y = posY;
            boardView = parent;

            image = GetComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.raycastTarget = true;
            image.color = EmptyColor;

            rt = GetComponent<RectTransform>();

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
            if (image == null) image = GetComponent<Image>();

            if (data == null)
            {
                image.sprite = GetWhiteSprite();
                image.color = EmptyColor;
                return;
            }

            x = data.x;
            y = data.y;

            int idx = (int)data.type;

            if (s_tileSprites != null && idx >= 0 && idx < s_tileSprites.Length && s_tileSprites[idx] != null)
            {
                image.sprite = s_tileSprites[idx];
                image.color = Color.white;
                return;
            }

            image.sprite = GetWhiteSprite();
            if (idx >= 0 && idx < TileColors.Length)
                image.color = TileColors[idx];
            else
                image.color = EmptyColor;
        }

        public void SetSelected(bool selected)
        {
            if (outline != null) outline.enabled = selected;
        }

        /// <summary>
        /// 드래그 종료 후 시각 위치를 그리드 원점으로 복귀.
        /// animate=true: 부드럽게 lerp (~180ms ease-out). 스왑 실패 시 사용.
        /// animate=false: 즉시 스냅. 스왑 성공 시 사용.
        /// </summary>
        public void SnapBackToOrigin(bool animate)
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            if (rt == null) return;

            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }

            if (!animate || !Application.isPlaying)
            {
                rt.anchoredPosition = originalAnchoredPos;
                return;
            }

            snapCoroutine = StartCoroutine(SnapBackCoroutine());
        }

        private IEnumerator SnapBackCoroutine()
        {
            Vector2 start = rt.anchoredPosition;
            Vector2 end = originalAnchoredPos;
            float t = 0f;
            while (t < SNAP_BACK_DURATION)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / SNAP_BACK_DURATION);
                // ease-out cubic
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                if (rt != null) rt.anchoredPosition = Vector2.Lerp(start, end, eased);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = end;
            snapCoroutine = null;
        }

        // ===== IPointerDownHandler =====

        public void OnPointerDown(PointerEventData eventData)
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            isPressed = true;
            dragInitiated = false;
            pressPos = eventData.position;
            originalAnchoredPos = rt != null ? rt.anchoredPosition : Vector2.zero;

            // 진행 중인 snap-back이 있으면 중단
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }

            if (boardView != null) boardView.OnTilePressed(this);
        }

        // ===== IBeginDragHandler / IDragHandler / IEndDragHandler =====

        public void OnBeginDrag(PointerEventData eventData)
        {
            // OnDrag가 호출되려면 IBeginDragHandler 구현이 필수.
            dragInitiated = isPressed;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragInitiated || rt == null) return;

            Vector2 delta = eventData.position - pressPos;
            // 카디널 방향만 허용 — 더 큰 축으로 스냅
            Vector2 cardinal = (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                ? new Vector2(delta.x, 0)
                : new Vector2(0, delta.y);

            // 한 타일 크기로 클램프 후 50%만 시각 적용
            float tileSize = Mathf.Min(rt.rect.width, rt.rect.height);
            if (tileSize <= 0f) tileSize = 100f;
            Vector2 limited = Vector2.ClampMagnitude(cardinal, tileSize) * DRAG_VISUAL_RATIO;
            rt.anchoredPosition = originalAnchoredPos + limited;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // PRIMARY release handler. OnPointerUp은 손 뗀 위치 타일에 fire되므로
            // 원래 press 타일을 추적하려면 OnEndDrag를 사용해야 한다.
            if (!isPressed) return;
            Vector2 delta = eventData.position - pressPos;
            isPressed = false;
            dragInitiated = false;
            // 위치는 BoardView가 결과에 따라 결정 — 여기서 리셋하지 않음.
            if (boardView != null) boardView.OnTileReleased(this, delta);
        }

        // ===== IPointerUpHandler =====

        public void OnPointerUp(PointerEventData eventData)
        {
            // 순수 탭(드래그 없음) 케이스 핸들러. 드래그가 있었으면 OnEndDrag가 이미 처리.
            // 손 뗀 위치가 다른 타일이면 이 콜백은 그 타일에 fire되므로 안전.
            if (!isPressed) return;
            if (dragInitiated) return; // OnEndDrag가 처리

            isPressed = false;
            dragInitiated = false;
            // 시각 변화는 없었으니 (OnDrag 미실행) 위치는 이미 origin. 그냥 BoardView에 zero delta 전달.
            if (boardView != null) boardView.OnTileReleased(this, Vector2.zero);
        }
    }
}
