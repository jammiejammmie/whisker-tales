using System.Collections.Generic;
using System.Reflection;
using GameVanilla.Core;
using GameVanilla.Game.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// Runtime guard for the Candy Match3 Kit GameBoard. Fired on every scene load,
    /// it sweeps GameBoard's private `gameSounds` List for null AudioClip entries
    /// (which would crash SoundManager.AddSounds with MissingReferenceException on
    /// `.name`) and also normalizes the singleton SoundManager.sounds list.
    ///
    /// Also scans every ObjectPool in the scene for a null `prefab` field — caused by
    /// missing-asset GUIDs in WhiskerGameScene's TilePool (e.g. unbreakablePool,
    /// marshmallowPool). A null prefab makes ObjectPool.Initialize blow up on
    /// Instantiate(null), which aborts GameBoard.InitializeObjectPools mid-foreach and
    /// leaves the remaining (candy) pools uninitialized, so no tiles spawn.
    ///
    /// To keep Initialize crash-free without touching Vendor assets, the scanner
    /// donates a sibling pool's healthy prefab (any non-null one in the same TilePool)
    /// to the broken pools. Level 1 never asks for the patched pool types
    /// (marshmallow/unbreakable), so the donated clones stay pooled and inert.
    ///
    /// Pattern follows AndroidUIMagentaHotfix: registered via RuntimeInitializeOnLoadMethod,
    /// hooks SceneManager.sceneLoaded, runs BEFORE GameBoard.Start() per Unity's documented
    /// sceneLoaded callback ordering (after Awake/OnEnable, before Start).
    /// </summary>
    public sealed class WhiskerGameBoardHotfix : MonoBehaviour
    {
        private static bool bootstrapped;
        private static FieldInfo gameSoundsField;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapped) return;
            bootstrapped = true;

            gameSoundsField = typeof(GameBoard).GetField(
                "gameSounds",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var go = new GameObject("__WhiskerGameBoardHotfix");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<WhiskerGameBoardHotfix>();
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Active scene was already loaded by the time we get here on first run.
            ScrubActiveScene("initial");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScrubScene(scene, $"sceneLoaded:{scene.name}");
        }

        private static void ScrubActiveScene(string phase)
        {
            ScrubScene(SceneManager.GetActiveScene(), phase);
        }

        private static void ScrubScene(Scene scene, string phase)
        {
            if (!scene.IsValid()) return;

            ScrubSoundManagerSingleton(phase);

            var roots = scene.GetRootGameObjects();
            // Patch ObjectPools BEFORE anything else — GameScene.Start calls
            // gameBoard.InitializeObjectPools() which iterates every pool and
            // calls Instantiate(prefab); a single null prefab aborts the loop.
            ScrubObjectPools(roots, phase);

            for (int i = 0; i < roots.Length; i++)
            {
                var boards = roots[i].GetComponentsInChildren<GameBoard>(true);
                for (int j = 0; j < boards.Length; j++)
                {
                    ScrubGameBoard(boards[j], phase);
                }
            }
        }

        private static void ScrubObjectPools(GameObject[] roots, string phase)
        {
            var allPools = new List<ObjectPool>();
            for (int i = 0; i < roots.Length; i++)
            {
                allPools.AddRange(roots[i].GetComponentsInChildren<ObjectPool>(true));
            }

            GameObject donor = null;
            for (int k = 0; k < allPools.Count; k++)
            {
                if (allPools[k].prefab != null)
                {
                    donor = allPools[k].prefab;
                    break;
                }
            }

            int patched = 0;
            for (int k = 0; k < allPools.Count; k++)
            {
                var pool = allPools[k];
                if (pool.prefab != null) continue;

                if (donor == null)
                {
                    Debug.LogWarning($"[GameBoardHotfix:{phase}] {pool.gameObject.name}.prefab is null and no healthy donor pool found — Initialize will still crash");
                    continue;
                }

                pool.prefab = donor;
                Debug.Log($"[GameBoardHotfix:{phase}] {pool.gameObject.name}.prefab was null → donor {donor.name}");
                patched++;
            }

            if (patched > 0)
            {
                Debug.Log($"[GameBoardHotfix:{phase}] patched {patched} ObjectPool(s) with donor prefab '{donor.name}'");
            }
        }

        private static void ScrubGameBoard(GameBoard board, string phase)
        {
            if (board == null || gameSoundsField == null) return;

            var list = gameSoundsField.GetValue(board) as List<AudioClip>;
            if (list == null)
            {
                gameSoundsField.SetValue(board, new List<AudioClip>());
                Debug.Log($"[GameBoardHotfix:{phase}] {board.gameObject.name}.gameSounds was null → reset to empty");
                return;
            }

            int removed = list.RemoveAll(clip => clip == null);
            if (removed > 0)
            {
                Debug.Log($"[GameBoardHotfix:{phase}] {board.gameObject.name}.gameSounds — removed {removed} null entries (kept {list.Count})");
            }
        }

        private static void ScrubSoundManagerSingleton(string phase)
        {
            var mgr = SoundManager.instance;
            if (mgr == null) return;

            if (mgr.sounds == null)
            {
                mgr.sounds = new List<AudioClip>();
                Debug.Log($"[GameBoardHotfix:{phase}] SoundManager.sounds was null → reset to empty");
                return;
            }

            int removed = mgr.sounds.RemoveAll(clip => clip == null);
            if (removed > 0)
            {
                Debug.Log($"[GameBoardHotfix:{phase}] SoundManager.sounds — removed {removed} null entries (kept {mgr.sounds.Count})");
            }
        }
    }
}
