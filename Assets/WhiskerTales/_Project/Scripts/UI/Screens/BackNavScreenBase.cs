using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI.Screens
{
    // Shared base for non-Home screens: serialized back button + navigator + home target id.
    // Concrete screens inherit and add their own content hooks.
    public abstract class BackNavScreenBase : UIScreenBase
    {
        [Header("Back Nav")]
        [SerializeField] protected Button backButton;
        [SerializeField] protected ScreenNavigator navigator;
        [SerializeField] protected string backTargetId = "home";

        protected override void Awake()
        {
            base.Awake();

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackPressed);
            }
        }

        public void SetNavigator(ScreenNavigator value)
        {
            navigator = value;
        }

        protected virtual void OnBackPressed()
        {
            if (navigator == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[" + GetType().Name + "] navigator missing; back ignored.");
                return;
            }

            if (string.IsNullOrEmpty(backTargetId) == true)
            {
                return;
            }

            navigator.Show(backTargetId);
        }
    }
}
