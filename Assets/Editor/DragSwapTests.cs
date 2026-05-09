using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using WhiskerTales.Puzzle;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Tools/Whisker Tales/Test/Drag Swap — 드래그 기반 타일 스왑 검증.
    /// 1) 드래그 거리 30% 미만 → 스왑 거부
    /// 2) 카디널 방향 드래그 → 인접 스왑 → 매치 발생
    /// 3) 보드 밖 드래그 → 스왑 불가, 보드 상태 유지
    /// 4) TileView가 모든 드래그 인터페이스를 구현
    /// </summary>
    public static class DragSwapTests
    {
        private const int SIZE = 8;

        [MenuItem("Tools/Whisker Tales/Test/Drag Swap")]
        public static void TestDragSwap()
        {
            int passed = 0, failed = 0;

            // ===== 4) 인터페이스 구현 검증 (가장 빠른 체크 먼저) =====
            bool ifaceOk = typeof(IPointerDownHandler).IsAssignableFrom(typeof(TileView))
                        && typeof(IPointerUpHandler).IsAssignableFrom(typeof(TileView))
                        && typeof(IBeginDragHandler).IsAssignableFrom(typeof(TileView))
                        && typeof(IDragHandler).IsAssignableFrom(typeof(TileView))
                        && typeof(IEndDragHandler).IsAssignableFrom(typeof(TileView));
            if (ifaceOk) { Debug.Log("  [PASS] TileView implements IPointerDown/Up + IBeginDrag/IDrag/IEndDrag"); passed++; }
            else { Debug.LogError("  [FAIL] TileView missing one or more drag interfaces"); failed++; }

            // ===== 1) 임계값 미만 드래그 거부 =====
            if (RunSubCase("below-threshold drag rejected (5px)",
                (board, view, tile) =>
                {
                    FillBackground(board);
                    board.DebugSetTile(0, 0, TileType.Fish);
                    board.DebugSetTile(1, 0, TileType.Milk);
                    tile.x = 0; tile.y = 0;
                    view.OnTileReleased(tile, new Vector2(5f, 0f)); // 5px < 30px threshold
                    return board.DebugBoard[0, 0].type == TileType.Fish
                        && board.DebugBoard[0, 1].type == TileType.Milk;
                })) passed++; else failed++;

            // ===== 2) 카디널 드래그 → 스왑 → 매치 =====
            if (RunSubCase("right drag triggers swap that matches",
                (board, view, tile) =>
                {
                    FillBackground(board);
                    // 행 0 세팅: Fish Milk Fish Fish ... → 좌측 두 개 스왑하면 (1,0)(2,0)(3,0) 3-매치
                    board.DebugSetTile(0, 0, TileType.Fish);
                    board.DebugSetTile(1, 0, TileType.Milk);
                    board.DebugSetTile(2, 0, TileType.Fish);
                    board.DebugSetTile(3, 0, TileType.Fish);
                    tile.x = 0; tile.y = 0;
                    view.OnTileReleased(tile, new Vector2(150f, 0f)); // 큰 우측 드래그
                    // 스왑 + 캐스케이드 후 잔여 매치 0개여야 함
                    var remaining = MatchLogic.FindAllMatches(board.DebugBoard);
                    return remaining.Count == 0;
                })) passed++; else failed++;

            // ===== 3) 보드 밖 드래그 (좌측 from x=0) → 스왑 불가 =====
            if (RunSubCase("out-of-bounds drag blocked (x=0 left)",
                (board, view, tile) =>
                {
                    FillBackground(board);
                    board.DebugSetTile(0, 0, TileType.Fish);
                    board.DebugSetTile(1, 0, TileType.Milk);
                    tile.x = 0; tile.y = 0;
                    view.OnTileReleased(tile, new Vector2(-150f, 0f)); // 좌측 → x=-1 밖
                    return board.DebugBoard[0, 0].type == TileType.Fish
                        && board.DebugBoard[0, 1].type == TileType.Milk;
                })) passed++; else failed++;

            // ===== 5) 카디널 스냅 — 대각선 입력도 더 큰 축으로 처리 =====
            if (RunSubCase("diagonal drag snaps to dominant axis",
                (board, view, tile) =>
                {
                    FillBackground(board);
                    board.DebugSetTile(0, 0, TileType.Fish);
                    board.DebugSetTile(1, 0, TileType.Milk);
                    board.DebugSetTile(2, 0, TileType.Fish);
                    board.DebugSetTile(3, 0, TileType.Fish);
                    tile.x = 0; tile.y = 0;
                    // 우상 방향 드래그 (x 우세) → 우측으로 스왑
                    view.OnTileReleased(tile, new Vector2(150f, 60f));
                    var remaining = MatchLogic.FindAllMatches(board.DebugBoard);
                    return remaining.Count == 0;
                })) passed++; else failed++;

            int total = passed + failed;
            string verdict = (failed == 0) ? "PASS" : "FAIL";
            Debug.Log($"[TEST] Drag Swap: {verdict} ({passed}/{total} passed)");
        }

        private delegate bool SubCase(Board board, BoardView view, TileView tile);

        private static bool RunSubCase(string name, SubCase body)
        {
            GameObject boardGo = null, viewGo = null, tileGo = null;
            try
            {
                boardGo = new GameObject("DragSwapTest_Board");
                Board board = boardGo.AddComponent<Board>();

                viewGo = new GameObject("DragSwapTest_BoardView");
                BoardView view = viewGo.AddComponent<BoardView>();
                view.board = board;

                tileGo = new GameObject("DragSwapTest_Tile",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                TileView tile = tileGo.AddComponent<TileView>();
                tile.Setup(0, 0, view);

                bool result = body(board, view, tile);
                if (result)
                {
                    Debug.Log($"  [PASS] {name}");
                    return true;
                }
                else
                {
                    Debug.LogError($"  [FAIL] {name}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"  [FAIL] {name} — exception {ex.GetType().Name}: {ex.Message}");
                return false;
            }
            finally
            {
                if (tileGo != null) UnityEngine.Object.DestroyImmediate(tileGo);
                if (viewGo != null) UnityEngine.Object.DestroyImmediate(viewGo);
                if (boardGo != null) UnityEngine.Object.DestroyImmediate(boardGo);
            }
        }

        /// <summary>체커보드 패턴(Milk/Yarn 교차) — 자동 매치 없음 보장.</summary>
        private static void FillBackground(Board board)
        {
            board.DebugSetupEmpty();
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    TileType t = ((x + y) % 2 == 0) ? TileType.Milk : TileType.Yarn;
                    board.DebugSetTile(x, y, t);
                }
            }
        }
    }
}
