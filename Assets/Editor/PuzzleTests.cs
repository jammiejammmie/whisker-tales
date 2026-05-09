using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WhiskerTales.Puzzle;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL 테스트 — Tools/Whisker Tales/Test 메뉴.
    /// 사용자 직접 플레이 없이 매치-3 핵심 로직 검증.
    /// </summary>
    public static class PuzzleTests
    {
        private const int SIZE = 8;

        // ===== ① CASCADE =====

        [MenuItem("Tools/Whisker Tales/Test/Cascade")]
        public static void TestCascade()
        {
            const string name = "Cascade";
            GameObject go = new GameObject($"PuzzleTest_{name}");
            try
            {
                Board board = go.AddComponent<Board>();
                FillBackground(board);

                // 강제 매치 주입: 행 0의 (0,0), (1,0), (2,0)을 모두 Fish로 (배경에는 Fish 없음 → 새 매치)
                board.DebugSetTile(0, 0, TileType.Fish);
                board.DebugSetTile(1, 0, TileType.Fish);
                board.DebugSetTile(2, 0, TileType.Fish);

                // 캐스케이드 루프 실행
                int cascade = board.DebugProcessExistingMatches();

                // 검증 1: 적어도 한 iter는 돌았어야 함
                if (cascade < 1)
                {
                    Fail(name, $"cascade ran {cascade} iter — expected >= 1");
                    return;
                }

                // 검증 2: 종료 후 매치가 남아있지 않아야 함
                List<List<TileData>> remaining = MatchLogic.FindAllMatches(board.DebugBoard);
                if (remaining.Count > 0)
                {
                    Fail(name, $"{remaining.Count} match group(s) remain after cascade — loop didn't fully resolve");
                    return;
                }

                // 검증 3: 보드의 모든 칸이 채워져 있어야 함
                for (int y = 0; y < SIZE; y++)
                {
                    for (int x = 0; x < SIZE; x++)
                    {
                        if (board.DebugBoard[y, x] == null)
                        {
                            Fail(name, $"empty cell at ({x},{y}) after cascade — FillEmpty didn't run");
                            return;
                        }
                    }
                }

                Pass(name, $"resolved in {cascade} iter, board fully filled, no remaining matches");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // ===== ② SPECIAL TILE CREATION =====

        [MenuItem("Tools/Whisker Tales/Test/Special Tile Creation")]
        public static void TestSpecialTileCreation()
        {
            int passed = 0, failed = 0;

            if (TestSpecialCase("4-horizontal → RocketHorizontal",
                b => SetLine(b, 0, 0, 4, true, TileType.Fish),
                SpecialItemType.RocketHorizontal)) passed++; else failed++;

            if (TestSpecialCase("4-vertical → RocketVertical",
                b => SetLine(b, 0, 0, 4, false, TileType.Fish),
                SpecialItemType.RocketVertical)) passed++; else failed++;

            if (TestSpecialCase("5-horizontal → Rainbow",
                b => SetLine(b, 0, 0, 5, true, TileType.Fish),
                SpecialItemType.Rainbow)) passed++; else failed++;

            if (TestSpecialCase("5-vertical → Rainbow",
                b => SetLine(b, 0, 0, 5, false, TileType.Fish),
                SpecialItemType.Rainbow)) passed++; else failed++;

            if (TestSpecialCase("2x2 square → RocketHorizontal",
                b => SetSquare(b, 0, 0, TileType.Fish),
                SpecialItemType.RocketHorizontal)) passed++; else failed++;

            // Note: 5 L/T → Bomb 테스트는 현재 FindAllMatches가 row+col을 별도 그룹으로 분리해
            // 5개 단일 그룹으로 모이지 않는 한계가 있음. 후속 작업 (FindAllMatches에 L/T 결합 로직).

            int total = passed + failed;
            string verdict = (failed == 0) ? "PASS" : "FAIL";
            Debug.Log($"[TEST] Special Tile Creation: {verdict} ({passed}/{total} passed)");
        }

        // ===== Helpers =====

        private static bool TestSpecialCase(string name, Action<Board> setup, SpecialItemType expected)
        {
            GameObject go = new GameObject($"SpecialTest_{name}");
            try
            {
                Board board = go.AddComponent<Board>();
                FillBackground(board);
                setup(board);

                // 단 1번 iter — survivor에 special이 placement된 직후 상태 검사
                board.DebugProcessExistingMatches(maxIterations: 1);

                bool found = false;
                for (int y = 0; y < SIZE; y++)
                {
                    for (int x = 0; x < SIZE; x++)
                    {
                        TileData t = board.DebugBoard[y, x];
                        if (t != null && t.specialItem == expected) { found = true; break; }
                    }
                    if (found) break;
                }

                if (found)
                {
                    Debug.Log($"  [PASS] {name}");
                    return true;
                }
                else
                {
                    SpecialItemType seen = ScanFirstNonNoneSpecial(board.DebugBoard);
                    Debug.LogError($"  [FAIL] {name} — expected {expected}, found {seen} on board");
                    return false;
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>체커보드 패턴(Milk/Yarn 교차)으로 보드를 채움 — 자동 매치 없음 보장.</summary>
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

        /// <summary>(startX, startY)부터 N개를 가로(horizontal=true)/세로로 같은 타입으로 배치.</summary>
        private static void SetLine(Board board, int startX, int startY, int count, bool horizontal, TileType type)
        {
            for (int i = 0; i < count; i++)
            {
                int x = horizontal ? startX + i : startX;
                int y = horizontal ? startY : startY + i;
                board.DebugSetTile(x, y, type);
            }
        }

        /// <summary>(x, y) 좌상단 기준 2×2 정사각을 같은 타입으로 배치.</summary>
        private static void SetSquare(Board board, int x, int y, TileType type)
        {
            board.DebugSetTile(x,     y,     type);
            board.DebugSetTile(x + 1, y,     type);
            board.DebugSetTile(x,     y + 1, type);
            board.DebugSetTile(x + 1, y + 1, type);
        }

        private static SpecialItemType ScanFirstNonNoneSpecial(TileData[,] b)
        {
            for (int y = 0; y < SIZE; y++)
                for (int x = 0; x < SIZE; x++)
                    if (b[y, x] != null && b[y, x].specialItem != SpecialItemType.None)
                        return b[y, x].specialItem;
            return SpecialItemType.None;
        }

        private static void Pass(string name, string detail) => Debug.Log($"[TEST] {name}: PASS — {detail}");
        private static void Fail(string name, string detail) => Debug.LogError($"[TEST] {name}: FAIL — {detail}");
    }
}
