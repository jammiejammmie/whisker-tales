// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the spin wheel prize popup.
    /// </summary>
    public class SpinWheelPrizePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private List<Sprite> sprites;
        
        [SerializeField]
        private Image image;
        
        [SerializeField]
        private Text text;
#pragma warning restore 649
       
        /// <summary>
        /// Sets the information of this popup.
        /// </summary>
        /// <param name="item">Associated spin wheel item.</param>
        public void SetInfo(SpinWheelItem item)
        {
            image.sprite = sprites[(int)item.type];
            text.text = item.amount.ToString();
        }
       
        /// <summary>
        /// Called when the button is pressed.
        /// </summary>
        public void OnButtonPressed()
        {
            Close();
        }
    }
}
