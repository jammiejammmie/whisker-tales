using System;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Cat;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Cat Room 탭 패널 (Stage 4 §4-2 진입점).
    /// 5마리 선택 그리드 → CatBondScreen.SetCat()으로 교감 화면 전환.
    /// </summary>
    public class CatRoomPanel : MonoBehaviour
    {
        [Serializable]
        public class CatEntry
        {
            public int catId;
            public Button selectButton;
            public GameObject lockedOverlay;
        }

        [Header("Sub Panels")]
        [SerializeField] private GameObject roomRoot;
        [SerializeField] private GameObject bondScreenRoot;
        [SerializeField] private CatBondScreen bondScreen;

        [Header("Cat Selection (5 cats)")]
        [SerializeField] private CatEntry[] entries = new CatEntry[5];

        [Header("Back to Room")]
        [SerializeField] private Button backFromBondButton;

        private void OnEnable()
        {
            foreach (var e in entries)
            {
                if (e == null || e.selectButton == null) continue;
                int captured = e.catId;
                e.selectButton.onClick.AddListener(() => OpenBond(captured));
            }
            if (backFromBondButton != null) backFromBondButton.onClick.AddListener(ShowRoom);

            RefreshLockState();
            ShowRoom();
        }

        private void OnDisable()
        {
            foreach (var e in entries)
            {
                if (e == null || e.selectButton == null) continue;
                e.selectButton.onClick.RemoveAllListeners();
            }
            if (backFromBondButton != null) backFromBondButton.onClick.RemoveListener(ShowRoom);
        }

        private void RefreshLockState()
        {
            if (CatManager.Instance == null) return;
            foreach (var e in entries)
            {
                if (e == null) continue;
                bool unlocked = CatManager.Instance.IsCatUnlocked(e.catId);
                if (e.lockedOverlay != null) e.lockedOverlay.SetActive(!unlocked);
                if (e.selectButton != null) e.selectButton.interactable = unlocked;
            }
        }

        private void OpenBond(int catId)
        {
            AudioManager.instance?.PlayButtonClick();
            if (bondScreen != null) bondScreen.SetCat(catId);
            if (roomRoot != null) roomRoot.SetActive(false);
            if (bondScreenRoot != null) bondScreenRoot.SetActive(true);
        }

        private void ShowRoom()
        {
            AudioManager.instance?.PlayButtonClick();
            if (bondScreenRoot != null) bondScreenRoot.SetActive(false);
            if (roomRoot != null) roomRoot.SetActive(true);
            RefreshLockState();
        }
    }
}
