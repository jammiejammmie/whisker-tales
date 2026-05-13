namespace WhiskerTales.UI
{
    // Mutable static handoff between LevelSelect (writer) and Gameplay (reader). Survives the
    // screen-to-screen navigation without coupling controllers. V2-8 stores just the level id;
    // future phases can extend with move-limit overrides, lifelines, etc.
    public static class GameplaySession
    {
        public const int DefaultLevelId = 1;

        public static int SelectedLevelId = DefaultLevelId;
    }
}
