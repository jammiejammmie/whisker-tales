using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WhiskerTales.Utilities;
using WhiskerTales.UI;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 개별 타일 데이터 및 상태 관리
    /// 보드의 각 셀을 나타냄
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Image tileImage;
        [SerializeField] private Image specialItemImage;
        [SerializeField] private Image obstacleImage;
        [SerializeField] private Button tileButton;

        // 위치
        public int x { get; set; }
        public int y { get; set; }

        // 타일 타입
        public TileType type { get; private set; }

        // 특수 아이템
        public SpecialItemType specialItem { get; private set; }

        // 장애물
        public ObstacleType obstacle { get; private set; }
        public int obstacleHealth { get; private set; }

        // 상태
        public bool isMatched { get; set; }
        public bool isMoving { get; set; }
        public bool isLocked { get; set; }
        public bool isSelected { get; set; }

        // 색상 맵
        private static readonly Color[] TileColors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f, 1f),     // Red
            new Color(0.2f, 0.5f, 1f, 1f),     // Blue
            new Color(0.2f, 1f, 0.2f, 1f),     // Green
            new Color(1f, 1f, 0.2f, 1f),       // Yellow
            new Color(1f, 0.5f, 1f, 1f),       // Purple
            new Color(1f, 0.7f, 0.2f, 1f)      // Orange
        };

        private Board board;

        private void OnEnable()
        {
            if (tileButton != null)
            {
                tileButton.onClick.AddListener(OnTileClicked);
            }
        }

        private void OnDisable()
        {
            if (tileButton != null)
            {
                tileButton.onClick.RemoveListener(OnTileClicked);
            }
        }

        /// <summary>
        /// 타일 초기화
        /// </summary>
        public void Initialize(int posX, int posY)
        {
            x = posX;
            y = posY;
            type = TileType.Empty;
            specialItem = SpecialItemType.None;
            obstacle = ObstacleType.Box;
            obstacleHealth = 0;
            isMatched = false;
            isMoving = false;
            isLocked = false;
            isSelected = false;

            board = FindObjectOfType<Board>();

            if (tileImage == null)
            {
                tileImage = GetComponent<Image>();
            }

            Debug.Log($"[Tile] Initialized at ({x}, {y})");
        }

        /// <summary>
        /// 타일 타입 설정
        /// </summary>
        public void SetType(TileType newType)
        {
            type = newType;
            UpdateVisuals();
            Debug.Log($"[Tile] Type set to {newType} at ({x}, {y})");
        }

        /// <summary>
        /// 특수 아이템 설정
        /// </summary>
        public void SetSpecialItem(SpecialItemType item)
        {
            specialItem = item;
            UpdateVisuals();
            Debug.Log($"[Tile] Special item set to {item} at ({x}, {y})");
        }

        /// <summary>
        /// 장애물 추가
        /// </summary>
        public void AddObstacle(ObstacleType obs, int health = 1)
        {
            obstacle = obs;
            obstacleHealth = health;
            UpdateVisuals();
            Debug.Log($"[Tile] Obstacle added: {obs} (health: {health}) at ({x}, {y})");
        }

        /// <summary>
        /// 장애물 제거
        /// </summary>
        public void RemoveObstacle()
        {
            obstacle = ObstacleType.Box;
            obstacleHealth = 0;
            UpdateVisuals();
            Debug.Log($"[Tile] Obstacle removed at ({x}, {y})");
        }

        /// <summary>
        /// 장애물 피해
        /// </summary>
        public void DamageObstacle()
        {
            if (obstacleHealth > 0)
            {
                obstacleHealth--;
                if (obstacleHealth == 0)
                {
                    RemoveObstacle();
                }
                UpdateVisuals();
                Debug.Log($"[Tile] Obstacle damaged at ({x}, {y}). Health: {obstacleHealth}");
            }
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            // 타일 색상 설정
            if (type != TileType.Empty && type != TileType.Obstacle)
            {
                tileImage.color = TileColors[(int)type];
            }
            else
            {
                tileImage.color = Color.gray;
            }

            // 특수 아이템 표시
            if (specialItemImage != null)
            {
                specialItemImage.gameObject.SetActive(specialItem != SpecialItemType.None);
                if (specialItem != SpecialItemType.None)
                {
                    specialItemImage.color = GetSpecialItemColor();
                }
            }

            // 장애물 표시
            if (obstacleImage != null)
            {
                obstacleImage.gameObject.SetActive(obstacleHealth > 0);
                if (obstacleHealth > 0)
                {
                    obstacleImage.color = GetObstacleColor();
                }
            }
        }

        /// <summary>
        /// 특수 아이템 색상 반환
        /// </summary>
        private Color GetSpecialItemColor()
        {
            return specialItem switch
            {
                SpecialItemType.Rocket => Color.red,
                SpecialItemType.Bomb => Color.black,
                SpecialItemType.Rainbow => Color.magenta,
                SpecialItemType.Hammer => Color.yellow,
                _ => Color.white
            };
        }

        /// <summary>
        /// 장애물 색상 반환
        /// </summary>
        private Color GetObstacleColor()
        {
            return obstacle switch
            {
                ObstacleType.Box => Color.brown,
                ObstacleType.Ice => Color.cyan,
                ObstacleType.Lock => new Color(1f, 0.84f, 0f, 1f), // Gold
                ObstacleType.Chain => Color.gray,
                _ => Color.white
            };
        }

        /// <summary>
        /// 타일 클릭 처리
        /// </summary>
        private void OnTileClicked()
        {
            if (board == null)
                return;

            // 게임 플레이 UI에서 처리
            GameplayUI gameplayUI = FindObjectOfType<GameplayUI>();
            if (gameplayUI != null)
            {
                gameplayUI.OnTileClicked(this);
            }

            Debug.Log($"[Tile] Clicked at ({x}, {y})");
        }

        /// <summary>
        /// 타일 이동
        /// </summary>
        public void MoveTo(int newX, int newY)
        {
            x = newX;
            y = newY;
            Debug.Log($"[Tile] Moved to ({x}, {y})");
        }

        /// <summary>
        /// 제거 애니메이션
        /// </summary>
        public void AnimateRemoval()
        {
            StartCoroutine(RemovalCoroutine());
        }

        /// <summary>
        /// 제거 애니메이션 코루틴
        /// </summary>
        private IEnumerator RemovalCoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0f;

            Vector3 startScale = transform.localScale;
            Color startColor = tileImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 스케일 축소
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

                // 페이드 아웃
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(1f, 0f, t);
                tileImage.color = newColor;

                yield return null;
            }

            transform.localScale = Vector3.zero;
            tileImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        }

        /// <summary>
        /// 선택 상태 표시
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (selected)
            {
                // 선택 상태 강조
                tileImage.color = Color.white;
                transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                // 선택 해제
                UpdateVisuals();
                transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 타일 정보 반환
        /// </summary>
        public override string ToString()
        {
            return $"Tile({x}, {y}) - Type: {type}, Special: {specialItem}, Obstacle: {obstacle}";
        }
    }
}
