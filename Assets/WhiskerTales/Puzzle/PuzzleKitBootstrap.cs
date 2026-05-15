using System.Collections.Generic;
using GameVanilla.Core;
using GameVanilla.Game.Common;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// Creates the Candy Match3 Kit singletons (PuzzleMatchManager + SoundManager) at app
    /// startup so WhiskerGameScene can be entered standalone, without booting through the
    /// Kit's HomeScene. The Kit's HomeScene/GameManager prefab has broken script GUIDs in
    /// this project, so we never instantiate it — we build the GameObjects from scratch.
    ///
    /// LivesSystem and CoinsSystem are added BEFORE PuzzleMatchManager so its Awake can
    /// resolve them via GetComponent.
    /// </summary>
    public static class PuzzleKitBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSingletons()
        {
            // Defensive: RuntimeInitializeOnLoadMethod(BeforeSceneLoad) only fires once
            // Play mode is starting, but guard anyway so no GameObject is ever spawned
            // in Edit mode under unusual domain-reload scenarios.
            if (!Application.isPlaying)
            {
                return;
            }
            EnsurePuzzleMatchManager();
            EnsureSoundManager();
        }

        private static void EnsurePuzzleMatchManager()
        {
            if (PuzzleMatchManager.instance != null)
            {
                return;
            }
            var go = new GameObject("__PuzzleMatchManager");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<LivesSystem>();
            go.AddComponent<CoinsSystem>();
            go.AddComponent<PuzzleMatchManager>();
            if (PuzzleMatchManager.instance != null && PuzzleMatchManager.instance.lastSelectedLevel == 0)
            {
                PuzzleMatchManager.instance.lastSelectedLevel = 1;
            }
            Debug.Log("[PuzzleKitBootstrap] PuzzleMatchManager singleton created (lastSelectedLevel=1)");
        }

        private static void EnsureSoundManager()
        {
            if (SoundManager.instance != null)
            {
                return;
            }
            var go = new GameObject("__SoundManager");
            Object.DontDestroyOnLoad(go);
            var mgr = go.AddComponent<SoundManager>();
            // SoundManager.Start() iterates `sounds` with foreach — null throws NRE on
            // bare singletons. Assign an empty list so Start completes cleanly; GameBoard
            // will AddSounds(gameSounds) later to populate nameToSound.
            mgr.sounds = new List<AudioClip>();
            Debug.Log("[PuzzleKitBootstrap] SoundManager singleton created (sounds=[] — GameBoard will AddSounds at scene load)");
        }
    }
}
