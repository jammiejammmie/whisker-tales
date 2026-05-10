using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class BottomNavRuntimeBinder : MonoBehaviour
    {
        [SerializeField] private PhoneVisibleSceneInstaller installer;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button catRoomButton;
        [SerializeField] private Button cafeButton;
        [SerializeField] private Button arcadeButton;
        [SerializeField] private Button meditationButton;

        private void Awake()
        {
            if (installer == null)
            {
                installer = FindObjectOfType<PhoneVisibleSceneInstaller>();
            }

            Bind();
        }

        private void Bind()
        {
            if (installer == null)
            {
                DebugLogger.Warning(LogCategory.UI, "BottomNavRuntimeBinder missing installer.");
                return;
            }

            BindButton(homeButton, installer.ShowHome);
            BindButton(catRoomButton, installer.ShowCatBonding);
            BindButton(cafeButton, installer.ShowCafeRestoration);
            BindButton(meditationButton, installer.ShowMeditation);

            if (arcadeButton != null)
            {
                arcadeButton.onClick.RemoveAllListeners();
                arcadeButton.onClick.AddListener(() => DebugLogger.Info(LogCategory.UI, "Arcade tab pressed. Arcade prefab not active yet."));
            }
        }

        private void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
