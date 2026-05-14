// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the daily bonus popup.
    /// </summary>
    public class DailyBonusPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private List<DailyBonusWidget> items;
#pragma warning restore 649

        private DailyBonusType currentType;
        private int currentAmount;

        /// <summary>
        /// Called when the obtain button is pressed.
        /// </summary>
        public void OnButtonPressed()
        {
            switch (currentType)
            {
                case DailyBonusType.Coins1:
                case DailyBonusType.Coins2:
                case DailyBonusType.Coins3:
                case DailyBonusType.Coins4:
                case DailyBonusType.Coins5:
                case DailyBonusType.Coins6:
                    PuzzleMatchManager.instance.coinsSystem.BuyCoins(currentAmount);
                    break;
                
                case DailyBonusType.Lives:
                    PuzzleMatchManager.instance.livesSystem.RefillLives();
                    break;

                case DailyBonusType.Lollipop:
                    IncreaseBoosters(0);
                    break;

                case DailyBonusType.Bomb:
                    IncreaseBoosters(1);
                    break;

                case DailyBonusType.Switch:
                    IncreaseBoosters(2);
                    break;
                
                case DailyBonusType.ColorBomb:
                    IncreaseBoosters(3);
                    break;
            }
            
            Close();
        }

        /// <summary>
        /// Sets the information of the popup.
        /// </summary>
        /// <param name="day">The day of the bonus.</param>
        public void SetInfo(int day)
        {
            Assert.IsTrue(day >= 0 && day < 7);
            var dailyBonus = PuzzleMatchManager.instance.gameConfig.dailyBonus;
            for (var i = 0; i < items.Count; i++)
            {
                items[i].SetInfo(dailyBonus[i].type, dailyBonus[i].amount, i <= day);
            }

            currentType = dailyBonus[day].type;
            currentAmount = dailyBonus[day].amount;
        }

        /// <summary>
        /// Helper method to increase the number of boosters of the given type.
        /// </summary>
        /// <param name="type">The type of booster to increase.</param>
        private void IncreaseBoosters(int type)
        {
            var playerPrefsKey = $"num_boosters_{type}";
            var numBoosters = PlayerPrefs.GetInt(playerPrefsKey);
            PlayerPrefs.SetInt(playerPrefsKey, numBoosters + currentAmount);
        }
    }
}
