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
    /// leaves the remaining (candy) pools uninitialized, so no tiles spawn. The scanner
    /// assigns a shared inert placeholder so Initialize completes; level 1 never asks
    /// for the broken pools anyway.
    ///
    /// Pattern follows AndroidUIMagentaHotfix: registered via RuntimeInitializeOnLoadMethod,
    /// hooks SceneManager.sceneLoaded, runs BEFORE GameBoard.Start() per Unity's documented
    /// sceneLoaded callback ordering (after Awake/OnEnable, before Start).
    /// </summary>
    public sealed class WhiskerGameBoardHotfix : MonoBehaviour
    {
        private static bool bootstrapped;
        private static FieldInfo gameSoundsField;
        private static GameObject placeholderPrefab;

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
            int patched = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                var pools = roots[i].GetComponentsInChildren<ObjectPool>(true);
                for (int j = 0; j < pools.Length; j++)
                {
                    var pool = pools[j];
                    if (pool.prefab == null)
                    {
                        pool.prefab = EnsurePlaceholderPrefab();
                        Debug.Log($"[GameBoardHotfix:{phase}] {pool.gameObject.name}.prefab was null → assigned __ObjectPoolPlaceholder");
                        patched++;
                    }
                }
            }
            if (patched > 0)
            {
                Debug.Log($"[GameBoardHotfix:{phase}] patched {patched} ObjectPool(s) with null prefab");
            }
        }

        private static GameObject EnsurePlaceholderPrefab()
        {
            if (placeholderPrefab != null) return placeholderPrefab;
            // Inactive so Instantiate clones it inactive — no rendering, no Awake side
            // effects on the children of the broken pool. DontDestroyOnLoad so it
            // survives scene transitions and stays valid for every future Initialize.
            placeholderPrefab = new GameObject("__ObjectPoolPlaceholder");
            placeholderPrefab.SetActive(false);
            Object.DontDestroyOnLoad(placeholderPrefab);
            return placeholderPrefab;
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
