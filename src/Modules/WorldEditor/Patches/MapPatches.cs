using HarmonyLib;
using Zordon.ScheduleI.WorldEditor.Features;
using MelonLoader;

namespace Zordon.ScheduleI.WorldEditor.Patches
{
    [HarmonyPatch("ScheduleOne.Map.Map", "Awake")]
    public static class MapPatches
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            MelonLogger.Msg("Map.Awake Postfix triggered. World Editor ready (Manual Load Mode).");
            // WorldPatchManager.Instance.ApplyPatches(); // Disabled for manual load workflow
        }
    }
}