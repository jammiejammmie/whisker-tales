// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// Represents a single item in the spin wheel.
    /// </summary>
    public class SpinWheelWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private List<Sprite> itemSprites;
        
        [SerializeField]
        private Image itemImage;
        
        [SerializeField]
        private Text amountText;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(itemImage);
            Assert.IsNotNull(amountText);
        }

        /// <summary>
        /// Sets the info of this widget.
        /// </summary>
        /// <param name="type">The daily bonus type.</param>
        /// <param name="amount">The daily bonus amount.</param>
        public void SetInfo(DailyBonusType type, int amount)
        {
            itemImage.sprite = itemSprites[(int)type];
            amountText.text = amount.ToString();
        }
    }
}
