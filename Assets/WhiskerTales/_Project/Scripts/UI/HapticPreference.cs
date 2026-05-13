namespace WhiskerTales.UI
{
    // Global enable flag for haptic feedback. Settings screen mutates this and persists via
    // SaveService; ButtonClickSfx consults it before invoking HapticManager.
    public static class HapticPreference
    {
        public static bool Enabled = true;
    }
}
