using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI.Home
{
    /// <summary>
    /// 홈 화면의 생활 오브젝트(방석/찻잔/퍼즐책) 슬롯 레이어.
    /// HomeObjectSet 데이터를 읽어 자식 RectTransform을 런타임에 spawn한다.
    /// sprite가 비어 있으면 시각은 표시하지 않고 터치 영역만(alpha 0.01) 둔다 —
    /// 채집사 발주 자산 도착 전 placeholder 금지 원칙.
    /// 방문 빛 영역(SleepMode 진입)은 SO가 아니라 prefab에 직접 배치한다.
    /// </summary>
    public sealed class HomeObjectLayer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private HomeObjectSet set;
        [SerializeField] private PhoneVisibleSceneInstaller installer;

        [Header("Door Light (SleepMode entry)")]
        [SerializeField] private RectTransform doorLightZone;
        [SerializeField] private Image sleepFlashOverlay;

        private RectTransform layerRect;

        private void Awake()
        {
            layerRect = GetComponent<RectTransform>();

            if (installer == null)
            {
                installer = FindObjectOfType<PhoneVisibleSceneInstaller>();
            }

            if (sleepFlashOverlay != null)
            {
                Color c = sleepFlashOverlay.color;
                c.a = 0.01f;
                sleepFlashOverlay.color = c;
            }
        }

        private void Start()
        {
            SpawnFromSet();
            HookDoorLight();
        }

        private void SpawnFromSet()
        {
            if (set == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeObjectLayer.set not assigned.");
                return;
            }

            HomeObjectEntry[] entries = set.Entries;

            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                HomeObjectEntry entry = entries[i];

                if (entry == null)
                {
                    continue;
                }

                SpawnEntry(entry);
            }
        }

        private void SpawnEntry(HomeObjectEntry entry)
        {
            GameObject go = new GameObject(string.IsNullOrEmpty(entry.id) == true ? "HomeObject" : "HomeObject_" + entry.id, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(layerRect, false);
            rect.anchorMin = entry.anchorMin;
            rect.anchorMax = entry.anchorMax;
            rect.pivot = entry.pivot;
            rect.sizeDelta = entry.size;
            rect.anchoredPosition = entry.anchoredPosition;

            Image image = go.AddComponent<Image>();
            image.raycastTarget = entry.interaction != HomeInteractionTarget.None;

            if (entry.sprite != null)
            {
                image.sprite = entry.sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else
            {
                image.sprite = null;
                Color c = Color.white;
                c.a = 0.01f;
                image.color = c;
            }

            if (entry.interaction != HomeInteractionTarget.None)
            {
                HomeInteractionRelay relay = go.AddComponent<HomeInteractionRelay>();
                relay.Configure(entry.interaction, this);
            }
        }

        private void HookDoorLight()
        {
            if (doorLightZone == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeObjectLayer.doorLightZone not assigned.");
                return;
            }

            Image image = doorLightZone.GetComponent<Image>();

            if (image == null)
            {
                image = doorLightZone.gameObject.AddComponent<Image>();
                Color c = Color.white;
                c.a = 0.01f;
                image.color = c;
            }

            image.raycastTarget = true;

            if (doorLightZone.GetComponent<HomeInteractionRelay>() == null)
            {
                HomeInteractionRelay relay = doorLightZone.gameObject.AddComponent<HomeInteractionRelay>();
                relay.Configure(HomeInteractionTarget.SleepMode, this);
            }
        }

        public void OnInteract(HomeInteractionTarget target, RectTransform sourceRect)
        {
            switch (target)
            {
                case HomeInteractionTarget.LevelSelect:
                    PlayPuzzleBookFeedback(sourceRect);
                    break;

                case HomeInteractionTarget.SleepMode:
                    PlaySleepFlashThenEnter();
                    break;
            }
        }

        private void PlayPuzzleBookFeedback(RectTransform sourceRect)
        {
            if (sourceRect == null)
            {
                EnterGameplay();
                return;
            }

            sourceRect.DOKill();
            Vector3 baseScale = Vector3.one;

            sourceRect
                .DOScale(baseScale * UILayoutConstants.HomePuzzleBookPressScale, UILayoutConstants.HomePuzzleBookPressSeconds)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    sourceRect.DOScale(baseScale, UILayoutConstants.HomePuzzleBookPressSeconds)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true)
                        .OnComplete(EnterGameplay);
                });
        }

        private void EnterGameplay()
        {
            if (installer == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeObjectLayer.installer not assigned — cannot enter gameplay.");
                return;
            }

            DebugLogger.Info(LogCategory.UI, "Home → Gameplay (puzzle book)");
            installer.ShowGameplay();
        }

        private void PlaySleepFlashThenEnter()
        {
            if (installer == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeObjectLayer.installer not assigned — cannot enter sleep mode.");
                return;
            }

            if (sleepFlashOverlay == null)
            {
                DebugLogger.Info(LogCategory.UI, "Home → SleepMode (no flash overlay)");
                installer.ShowSleepMode();
                return;
            }

            sleepFlashOverlay.DOKill();

            Color baseColor = sleepFlashOverlay.color;
            float half = UILayoutConstants.HomeSleepFlashSeconds * 0.5f;

            sleepFlashOverlay
                .DOFade(UILayoutConstants.HomeSleepFlashPeakAlpha, half)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    DebugLogger.Info(LogCategory.UI, "Home → SleepMode (door light)");
                    installer.ShowSleepMode();

                    sleepFlashOverlay
                        .DOFade(0.01f, half)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            Color c = sleepFlashOverlay.color;
                            c.a = 0.01f;
                            sleepFlashOverlay.color = c;
                        });
                });
        }
    }

    /// <summary>
    /// 자식 GameObject에 부착되어 클릭을 받아 부모 HomeObjectLayer에 라우팅한다.
    /// 자식이 직접 IPointerClickHandler를 가져야 raycast 이벤트를 받을 수 있다.
    /// </summary>
    public sealed class HomeInteractionRelay : MonoBehaviour, IPointerClickHandler
    {
        private HomeInteractionTarget target;
        private HomeObjectLayer layer;
        private RectTransform rect;

        public void Configure(HomeInteractionTarget interactionTarget, HomeObjectLayer parentLayer)
        {
            target = interactionTarget;
            layer = parentLayer;
            rect = transform as RectTransform;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (layer == null)
            {
                return;
            }

            layer.OnInteract(target, rect);
        }
    }
}
