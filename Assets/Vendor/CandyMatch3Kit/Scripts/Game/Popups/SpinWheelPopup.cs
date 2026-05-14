// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the spin wheel popup.
    /// </summary>
    public class SpinWheelPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Animator wheelAnimator;
        
        [SerializeField]
        private Image coinImage;
        
        [SerializeField]
        private Text costText;

        [SerializeField]
        private SpinWheelWidget blueDarkItem;
        
        [SerializeField]
        private SpinWheelWidget greenItem;
        
        [SerializeField]
        private SpinWheelWidget yellowItem;
        
        [SerializeField]
        private SpinWheelWidget orangeItem;
        
        [SerializeField]
        private SpinWheelWidget pinkItem;
        
        [SerializeField]
        private SpinWheelWidget redItem;
        
        [SerializeField]
        private SpinWheelWidget purpleItem;
        
        [SerializeField]
        private SpinWheelWidget blueLightItem;
#pragma warning restore 649
        
        private readonly string dateLastSpinKey = "date_last_spin";
        private readonly string numSpinsKey = "num_spins";
        
        private static readonly int BlueDark = Animator.StringToHash("BlueDark");
        private static readonly int Green = Animator.StringToHash("Green");
        private static readonly int Yellow = Animator.StringToHash("Yellow");
        private static readonly int Orange = Animator.StringToHash("Orange");
        private static readonly int Pink = Animator.StringToHash("Pink");
        private static readonly int Red = Animator.StringToHash("Red");
        private static readonly int Purple = Animator.StringToHash("Purple");
        private static readonly int BlueLight = Animator.StringToHash("BlueLight");

        private List<SpinWheelItem> spinWheelItems;
        private bool isSpinning;
        private int selectedItem;

        private bool isFreeSpin;
        private int spinCost;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(wheelAnimator);
            Assert.IsNotNull(coinImage);
            Assert.IsNotNull(costText);
            Assert.IsNotNull(blueDarkItem);
            Assert.IsNotNull(greenItem);
            Assert.IsNotNull(yellowItem);
            Assert.IsNotNull(orangeItem);
            Assert.IsNotNull(pinkItem);
            Assert.IsNotNull(redItem);
            Assert.IsNotNull(purpleItem);
            Assert.IsNotNull(blueLightItem);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            
            if (!PlayerPrefs.HasKey(dateLastSpinKey))
            {
                SetFreeSpin();
            }
            else
            {
                var dateLastSpinStr = PlayerPrefs.GetString(dateLastSpinKey);
                var dateLastSpin = Convert.ToDateTime(dateLastSpinStr, CultureInfo.InvariantCulture);

                var dateNow = DateTime.Now;
                var diff = dateNow.Subtract(dateLastSpin);
                if (diff.TotalHours >= 24)
                {
                    SetFreeSpin();
                }
                else
                {
                    SetSpinCost();
                }
            }
        }

        /// <summary>
        /// Configures the wheel for a free spin.
        /// </summary>
        private void SetFreeSpin()
        {
            isFreeSpin = true;
            coinImage.enabled = false;
            costText.enabled = false;
            PlayerPrefs.SetInt(numSpinsKey, 0);
        }

        /// <summary>
        /// Configures the wheel for a paid spin.
        /// </summary>
        private void SetSpinCost()
        {
            var gameConfig = PuzzleMatchManager.instance.gameConfig;
            var numSpins = PlayerPrefs.GetInt(numSpinsKey);
            
            spinCost = gameConfig.spinWheelCost;
            spinCost += (numSpins - 1) * gameConfig.spinWheelCostIncrement;
            costText.text = spinCost.ToString();
        }

        /// <summary>
        /// Sets the information of the spin wheel.
        /// </summary>
        /// <param name="items">The list of spin wheel items.</param>
        /// <param name="cost">The cost of the spin.</param>
        public void SetInfo(List<SpinWheelItem> items, int cost)
        {
            spinWheelItems = items;
            blueDarkItem.SetInfo(items[0].type, items[0].amount);
            greenItem.SetInfo(items[1].type, items[1].amount);
            yellowItem.SetInfo(items[2].type, items[2].amount);
            orangeItem.SetInfo(items[3].type, items[3].amount);
            pinkItem.SetInfo(items[4].type, items[4].amount);
            redItem.SetInfo(items[5].type, items[5].amount);
            purpleItem.SetInfo(items[6].type, items[6].amount);
            blueLightItem.SetInfo(items[7].type, items[7].amount);
            costText.text = cost.ToString();
        }
        
        /// <summary>
        /// Called when the spin button is pressed.
        /// </summary>
        public void OnSpinButtonPressed()
        {
            if (isSpinning)
            {
                return;
            }

            var numCoins = PlayerPrefs.GetInt("num_coins");
            if (isFreeSpin || numCoins >= spinCost)
            {
                if (!isFreeSpin)
                {
                    PuzzleMatchManager.instance.coinsSystem.SpendCoins(spinCost);
                }
                
                isSpinning = true;

                selectedItem = UnityEngine.Random.Range(0, 7);
                switch (selectedItem)
                {
                    case 0:
                        wheelAnimator.SetTrigger(BlueDark);
                        break;

                    case 1:
                        wheelAnimator.SetTrigger(Green);
                        break;

                    case 2:
                        wheelAnimator.SetTrigger(Yellow);
                        break;

                    case 3:
                        wheelAnimator.SetTrigger(Orange);
                        break;

                    case 4:
                        wheelAnimator.SetTrigger(Pink);
                        break;

                    case 5:
                        wheelAnimator.SetTrigger(Red);
                        break;

                    case 6:
                        wheelAnimator.SetTrigger(Purple);
                        break;

                    case 7:
                        wheelAnimator.SetTrigger(BlueLight);
                        break;
                }

                var numSpins = PlayerPrefs.GetInt(numSpinsKey);
                numSpins += 1;
                PlayerPrefs.SetInt(numSpinsKey, numSpins);

                PlayerPrefs.SetString(dateLastSpinKey, Convert.ToString(DateTime.Today, CultureInfo.InvariantCulture));
            }
            else
            {
                Close();
                parentScene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup");
            }
        }

        /// <summary>
        /// Called when the spin wheel animation is finished.
        /// </summary>
        public void OnSpinWheelAnimationFinished()
        {
            Close();
            
            AwardPrize();
            
            parentScene.OpenPopup<SpinWheelPrizePopup>("Popups/SpinWheelPrizePopup", popup =>
            {
                popup.SetInfo(spinWheelItems[selectedItem]);
            });
        }

        /// <summary>
        /// Rewards the player with the spin wheel prize.
        /// </summary>
        private void AwardPrize()
        {
            switch (spinWheelItems[selectedItem].type)
            {
                case DailyBonusType.Coins1:
                case DailyBonusType.Coins2:
                case DailyBonusType.Coins3:
                case DailyBonusType.Coins4:
                case DailyBonusType.Coins5:
                case DailyBonusType.Coins6:
                    PuzzleMatchManager.instance.coinsSystem.BuyCoins(spinWheelItems[selectedItem].amount);
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
        }
        
        /// <summary>
        /// Helper method to increase the number of boosters of the given type.
        /// </summary>
        /// <param name="type">The type of booster to increase.</param>
        private void IncreaseBoosters(int type)
        {
            var playerPrefsKey = $"num_boosters_{type}";
            var numBoosters = PlayerPrefs.GetInt(playerPrefsKey);
            PlayerPrefs.SetInt(playerPrefsKey, numBoosters + spinWheelItems[selectedItem].amount);
        }
    }
}
