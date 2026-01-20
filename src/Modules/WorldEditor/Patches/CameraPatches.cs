using HarmonyLib;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using Zordon.ScheduleI.WorldEditor.Features;

namespace Zordon.ScheduleI.WorldEditor.Patches
{
    [HarmonyPatch(typeof(PlayerCamera), "RotateFreeCam")]
    public static class CameraPatches
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // Only allow rotation if Right Mouse Button is held
            // This enables the cursor to be free for UI/Gizmos by default
            if (EditorController.Instance.IsEditorActive)
            {
                if (Input.GetMouseButton(1))
                {
                    return true; // Run original rotation
                }
                return false; // Skip original rotation
            }
            return true;
        }
    }
}
