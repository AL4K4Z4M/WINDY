using MelonLoader;

namespace WindyFramework.Core
{
    public static class WindyConfig
    {
        private static MelonPreferences_Category _category;
        public static MelonPreferences_Entry<bool> EnableSurvival;
        public static MelonPreferences_Entry<bool> EnableWorldEditor;
        public static MelonPreferences_Entry<bool> EnableUI;
        public static MelonPreferences_Entry<bool> EnableNPCGenerator;

        public static void Initialize()
        {
            _category = MelonPreferences.CreateCategory("WindyFW");
            EnableSurvival = _category.CreateEntry("EnableSurvival", true, "Enable Survival Module");
            EnableWorldEditor = _category.CreateEntry("EnableWorldEditor", true, "Enable World Editor Module");
            EnableUI = _category.CreateEntry("EnableUI", true, "Enable UI Module");
            EnableNPCGenerator = _category.CreateEntry("EnableNPCGenerator", true, "Enable NPC Generator Module");
            
            // Save config to ensure entries exist
            MelonPreferences.Save();
        }
    }
}
