// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using GameVanilla.Game.Popups;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// Utility class linked to the spin wheel that enables notifying the
    /// spin wheel popup from within an animation clip (via an animation
    /// event).
    /// </summary>
    public class SpinWheel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private SpinWheelPopup popup;
#pragma warning restore 649

        /// <summary>
        /// Called from the spin wheel animation when it finishes.
        /// </summary>
        public void OnSpinWheelAnimationFinished()
        {
            popup.OnSpinWheelAnimationFinished();
        }
    }
}
