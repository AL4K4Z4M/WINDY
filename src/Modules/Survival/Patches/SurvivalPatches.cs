using HarmonyLib;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts.Health;
using Zordon.ScheduleI.Survival.Features;
using UnityEngine;

namespace Zordon.ScheduleI.Survival.Patches
{
    [HarmonyPatch(typeof(PoliceStation), nameof(PoliceStation.Dispatch))]
    public static class PoliceStation_Dispatch_Patch
    {
        public static bool Prefix()
        {
            // Block all police dispatches if Survival mode is enabled
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.PlayerScripts.PlayerCrimeData), nameof(ScheduleOne.PlayerScripts.PlayerCrimeData.SetPursuitLevel))]
    public static class PlayerCrimeData_SetPursuitLevel_Patch
    {
        public static bool Prefix(ScheduleOne.PlayerScripts.PlayerCrimeData.EPursuitLevel level)
        {
            if (SurvivalController.Instance.SurvivalEnabled && level != ScheduleOne.PlayerScripts.PlayerCrimeData.EPursuitLevel.None)
            {
                return false; // Block setting wanted level to anything but None
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.PlayerScripts.PlayerCrimeData), nameof(ScheduleOne.PlayerScripts.PlayerCrimeData.AddCrime))]
    public static class PlayerCrimeData_AddCrime_Patch
    {
        public static bool Prefix()
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                return false; // Block adding crimes
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.Behaviour.CallPoliceBehaviour), nameof(ScheduleOne.NPCs.Behaviour.CallPoliceBehaviour.Activate))]
    public static class CallPoliceBehaviour_Activate_Patch
    {
        public static bool Prefix()
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                return false; // Block citizens from starting the call police behavior
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Cartel.CartelActivity), nameof(ScheduleOne.Cartel.CartelActivity.Activate))]
    public static class CartelActivity_Activate_Patch
    {
        public static bool Prefix()
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                return false; // Block cartel activities
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Combat.CombatBehaviour), "CheckTargetVisibility")]
    public static class CombatBehaviour_CheckTargetVisibility_Patch
    {
        public static void Postfix(ScheduleOne.Combat.CombatBehaviour __instance)
        {
            // If in survival mode, force the AI to always know where the player is if they are the target
            if (SurvivalController.Instance.SurvivalEnabled && __instance.Target != null && __instance.Target.IsPlayer)
            {
                // Access private fields via reflection to force "Recently Visible" state
                var fieldSighting = typeof(ScheduleOne.Combat.CombatBehaviour).GetField("timeSinceLastSighting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fieldVisionEvent = typeof(ScheduleOne.Combat.CombatBehaviour).GetField("visionEventReceived", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                fieldSighting?.SetValue(__instance, 0f);
                fieldVisionEvent?.SetValue(__instance, true);

                // Update the last known position to the actual current position
                var fieldLastPos = typeof(ScheduleOne.Combat.CombatBehaviour).GetField("lastKnownTargetPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fieldLastPos?.SetValue(__instance, __instance.Target.CenterPoint);
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Combat.CombatBehaviour), "IsTargetValid")]
    public static class CombatBehaviour_IsTargetValid_Patch
    {
        public static bool Prefix(ScheduleOne.Combat.CombatBehaviour __instance, ref bool __result)
        {
            if (SurvivalController.Instance.SurvivalEnabled && __instance.Target != null && __instance.Target.IsPlayer)
            {
                // Force target to be valid regardless of range or visibility
                if (!__instance.Target.IsNull() && __instance.Target.IsCurrentlyTargetable)
                {
                    __result = true;
                    return false; // Skip original check (which has GiveUpRange)
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Die))]
    public static class PlayerHealth_Die_Patch
    {
        public static bool Prefix(PlayerHealth __instance)
        {
            MelonLoader.MelonLogger.Msg($"[Survival] Die() called on {__instance.name}");

            // Only intercept if Survival is Enabled OR we are in the 'Lost' state waiting to reset
            if (SurvivalController.Instance.SurvivalEnabled || SurvivalController.Instance.IsLostState)
            {
                MelonLoader.MelonLogger.Msg("[Survival] Death Intercepted! Triggering OnPlayerLost.");
                
                if (SurvivalController.Instance.SurvivalEnabled)
                {
                    SurvivalController.Instance.OnPlayerLost();
                }
                
                // Return false to prevent the game's default "Die -> Reload Save" logic
                return false; 
            }
            return true;
        }
    }
}
