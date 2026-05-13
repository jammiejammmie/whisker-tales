using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Feel;

namespace WhiskerTales.UI
{
    // Polish layer: every Button on a V2 screen plays a soft click sfx + light haptic.
    // Auto-binds on Awake to its sibling Button so prefab builders just AddComponent and forget.
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class ButtonClickSfx : MonoBehaviour
    {
        [SerializeField] private SfxId sfx = SfxId.ButtonClick;
        [SerializeField] private bool hapticOnClick = true;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySfx(sfx);
            }

            if (hapticOnClick == true && HapticPreference.Enabled == true && HapticManager.Instance != null)
            {
                HapticManager.Instance.Light();
            }
        }
    }
}
