// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using GameVanilla.Game.Common;

namespace GameVanilla.Editor
{
    /// <summary>
    /// This class handles the Candy Match 3 Kit editor window.
    /// </summary>
    public class CandyMatch3KitEditor : EditorWindow
    {
        public GameConfiguration gameConfig;

        private readonly List<EditorTab> tabs = new List<EditorTab>();

        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;

        /// <summary>
        /// Static initialization of the editor window.
        /// </summary>
        [MenuItem("Tools/Candy Match 3 Kit/Editor", false, 0)]
        private static void Init()
        {
            var window = GetWindow(typeof(CandyMatch3KitEditor));
            window.titleContent = new GUIContent("Candy Match 3 Kit Editor");
        }

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        private void OnEnable()
        {
            tabs.Add(new GameSettingsTab(this));
            tabs.Add(new LevelEditorTab(this));
            tabs.Add(new AboutTab(this));
            selectedTabIndex = 0;
        }

        /// <summary>
        /// Unity's OnGUI method.
        /// </summary>
        private void OnGUI()
        {
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
                new[] {"Game settings", "Level editor", "About"});
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                    GUI.FocusControl(null);
                }
                selectedEditor.Draw();
                prevSelectedTabIndex = selectedTabIndex;
            }
        }
    }
}
