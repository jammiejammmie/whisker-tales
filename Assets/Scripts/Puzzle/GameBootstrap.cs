using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 씬에 부착되어 Awake 시점에 절차적으로 게임 화면을 구성한다.
    /// 의존성: 같은 어셈블리의 Board / LevelGoal / BoardView / TileView
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Level Settings")]
        public int moveLimit = 25;
        public int goalValue = 50;
        public LevelGoalType goalType = LevelGoalType.RemoveBlocks;

        private void Awake()
        {
            ConfigureCamera();
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();
            (TextMeshProUGUI goalText, TextMeshProUGUI movesText, TextMeshProUGUI statusText) = BuildHud(canvas.transform);
            RectTransform gridArea = BuildBoardArea(canvas.transform);

            // 데이터 레이어
            Board board = new GameObject("Board").AddComponent<Board>();
            LevelGoal levelGoal = new GameObject("LevelGoal").AddComponent<LevelGoal>();

            Level level = new Level
            {
                levelId = 1,
                moveLimit = this.moveLimit,
                goalType = this.goalType,
                goalValue = this.goalValue
            };
            board.Initialize(level, levelGoal);

            // 뷰 레이어
            BoardView view = new GameObject("BoardView").AddComponent<BoardView>();
            view.board = board;
            view.levelGoal = levelGoal;
            view.gridContainer = gridArea;
            view.goalText = goalText;
            view.movesText = movesText;
            view.statusText = statusText;
            view.BuildGrid();

            Debug.Log("[GameBootstrap] Scene constructed");
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
            cam.orthographic = false; // ScreenSpaceOverlay라 무관
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Canvas CreateCanvas()
        {
            GameObject go = new GameObject("Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private (TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI) BuildHud(Transform parent)
        {
            GameObject hud = new GameObject("HUD", typeof(RectTransform));
            RectTransform rt = hud.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -60);
            rt.sizeDelta = new Vector2(-80, 280);

            TextMeshProUGUI goal = CreateText(rt, "GoalText",
                new Vector2(0, 0.55f), new Vector2(1, 1), TextAlignmentOptions.Left, 56, "Goal");
            TextMeshProUGUI moves = CreateText(rt, "MovesText",
                new Vector2(0, 0.55f), new Vector2(1, 1), TextAlignmentOptions.Right, 56, "Moves");
            TextMeshProUGUI status = CreateText(rt, "StatusText",
                new Vector2(0, 0), new Vector2(1, 0.5f), TextAlignmentOptions.Center, 44, "");

            // 색 차별
            goal.color = new Color(0.85f, 0.95f, 1f);
            moves.color = new Color(1f, 0.92f, 0.65f);
            status.color = new Color(1f, 1f, 0.6f);

            return (goal, moves, status);
        }

        private TextMeshProUGUI CreateText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align,
            float fontSize, string defaultText)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = align;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private RectTransform BuildBoardArea(Transform parent)
        {
            // 보드 컨테이너 (정사각형, 화면 중앙)
            GameObject area = new GameObject("BoardArea",
                typeof(RectTransform),
                typeof(GridLayoutGroup));

            RectTransform rt = area.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(960, 960);

            GridLayoutGroup grid = area.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(112, 112);
            grid.spacing = new Vector2(8, 8);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            // 보드 배경
            Image bg = area.AddComponent<Image>();
            bg.sprite = TileView.GetWhiteSprite();
            bg.color = new Color(0.07f, 0.09f, 0.12f, 0.85f);
            bg.raycastTarget = false;

            return rt;
        }
    }
}
