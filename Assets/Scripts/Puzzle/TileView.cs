using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 단일 타일 1개를 화면에 표시하는 UI 컴포넌트
    /// 클릭 입력은 BoardView로 위임됨
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class TileView : MonoBehaviour, IPointerClickHandler
    {
        public int x;
        public int y;

        private Image image;
        private Outline outline;
        private BoardView boardView;

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
        // 외부에서 SetTileSprites로 주입. 미주입 시 TileColors fallback으로 렌더링됨.
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

        public void Setup(int posX, int posY, BoardView parent)
        {
            x = posX;
            y = posY;
            boardView = parent;

            image = GetComponent<Image>();
            image.sprite = GetWhiteSprite();
            image.raycastTarget = true;
            image.color = EmptyColor;

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

            // sprite 주입돼 있으면 sprite로 렌더, color는 흰색(틴팅 없음)
            if (s_tileSprites != null && idx >= 0 && idx < s_tileSprites.Length && s_tileSprites[idx] != null)
            {
                image.sprite = s_tileSprites[idx];
                image.color = Color.white;
                return;
            }

            // 미주입 fallback: 흰 sprite + 타일별 색상
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (boardView != null) boardView.OnTileClicked(this);
        }
    }
}
