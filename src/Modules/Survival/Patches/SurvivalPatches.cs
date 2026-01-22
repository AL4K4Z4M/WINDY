using HarmonyLib;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts.Health;
using Zordon.ScheduleI.Survival.Features;
using UnityEngine;
using System;

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
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Force-assign the backing field for TargetSighted to true
                var field = typeof(ScheduleOne.Combat.CombatBehaviour).GetField("<TargetSighted>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(__instance, true);
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

    [HarmonyPatch(typeof(ScheduleOne.AvatarFramework.Equipping.AvatarWeapon), nameof(ScheduleOne.AvatarFramework.Equipping.AvatarWeapon.Equip))]
    public static class AvatarWeapon_Equip_Patch
    {
        public static void Postfix(ScheduleOne.AvatarFramework.Equipping.AvatarWeapon __instance, ScheduleOne.AvatarFramework.Avatar _avatar)
        {
            // Giantism removed for enemies for now
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPC), nameof(ScheduleOne.NPCs.NPC.SendImpact))]
    public static class NPC_SendImpact_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.NPC __instance, ScheduleOne.Combat.Impact impact)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // If an NPC is hitting another NPC during survival, block it (Prevent friendly fire)
                if (impact != null && impact.ImpactSource != null)
                {
                    var attackerPlayer = impact.ImpactSource.GetComponent<ScheduleOne.PlayerScripts.Player>();
                    if (attackerPlayer == null) // It is an NPC attacking
                    {
                        // Current instance is the victim NPC
                        return false; 
                    }
                }
            }
            return true;
        }
    }

    // Since we can't easily patch the MoveNext of a compiler-generated iterator without its exact name,
    // we will use a more brute-force but effective approach:
    // Patch Physics.RaycastAll and check if the current thread/context is a Giant NPC attacking.
    // [GIGANTISM REMOVED]

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPC), nameof(ScheduleOne.NPCs.NPC.PlayVO))]
    public static class NPC_PlayVO_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.NPC __instance, ScheduleOne.VoiceOver.EVOLineType lineType)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Only block crime-related VO for cops
                if (__instance is ScheduleOne.Police.PoliceOfficer)
                {
                    if (lineType == ScheduleOne.VoiceOver.EVOLineType.Command || 
                        lineType == ScheduleOne.VoiceOver.EVOLineType.Angry || 
                        lineType == ScheduleOne.VoiceOver.EVOLineType.Annoyed)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Dialogue.DialogueHandler), nameof(ScheduleOne.Dialogue.DialogueHandler.PlayReaction))]
    public static class DialogueHandler_PlayReaction_Patch
    {
        public static bool Prefix(ScheduleOne.Dialogue.DialogueHandler __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                if (__instance.NPC is ScheduleOne.Police.PoliceOfficer)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPC), nameof(ScheduleOne.NPCs.NPC.SendWorldspaceDialogueReaction))]
    public static class NPC_SendWorldspaceDialogueReaction_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.NPC __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                if (__instance is ScheduleOne.Police.PoliceOfficer)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPCHealth), "Die")]
    public static class NPCHealth_Die_Boss_Patch
    {
        public static void Postfix(ScheduleOne.NPCs.NPCHealth __instance)
        {
            // STRICT SURVIVAL ONLY
            if (!SurvivalController.Instance.SurvivalEnabled) return;

            try 
            {
                var npc = __instance.GetComponent<ScheduleOne.NPCs.NPC>();
                if (npc != null && npc.Inventory != null)
                {
                    float loot = npc.Inventory.GetCashInInventory();
                    
                    if (loot > 0)
                    {
                        var player = ScheduleOne.PlayerScripts.Player.Local;
                        if (player != null)
                        {
                            var playerInv = player.GetComponent<ScheduleOne.PlayerScripts.PlayerInventory>();
                            if (playerInv != null && playerInv.cashInstance != null)
                            {
                                playerInv.cashInstance.ChangeBalance(loot);
                                npc.Inventory.RemoveCash(loot);
                                MelonLoader.MelonLogger.Msg($"[Survival] Auto-looted {loot} from {npc.fullName}");
                            }
                        }
                    }
                    
                    // Force HUD update on death
                    SurvivalController.Instance.Invoke("CleanupOrphanedLabels", 0.05f);
                    SurvivalController.Instance.Invoke("UpdateGroundTruthCache", 0.1f);
                }
            }
            catch (System.Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[Survival] Auto-loot Error: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPCHealth), "TakeDamage")]
    public static class NPCHealth_TakeDamage_Patch
    {
        public static void Prefix(ScheduleOne.NPCs.NPCHealth __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var npcField = typeof(ScheduleOne.NPCs.NPCHealth).GetField("npc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var npc = npcField?.GetValue(__instance) as ScheduleOne.NPCs.NPC;
                
                if (npc is ScheduleOne.NPCs.CharacterClasses.SewerGoblin)
                {
                    // Aggressively force invincibility off every time they are hit
                    __instance.Invincible = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPCHealth), "OnStartServer")]
    public static class NPCHealth_OnStartServer_Patch
    {
        public static void Postfix(ScheduleOne.NPCs.NPCHealth __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var npc = __instance.GetComponent<ScheduleOne.NPCs.NPC>();
                if (npc is ScheduleOne.NPCs.CharacterClasses.SewerGoblin)
                {
                    // Ensure the boss is NOT invincible during survival
                    __instance.Invincible = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin), nameof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin.DeployToPlayer))]
    public static class SewerGoblin_DeployToPlayer_Patch
    {
        public static void Postfix(ScheduleOne.NPCs.CharacterClasses.SewerGoblin __instance, ScheduleOne.PlayerScripts.Player player)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Force-assign the target player via reflection to be 100% sure it's set
                var field = typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin).GetField("<TargetPlayer>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(__instance, player);
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin), "CanBeginRetieve")]
    public static class SewerGoblin_CanBeginRetieve_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.CharacterClasses.SewerGoblin __instance, ref bool __result)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // If TargetPlayer is null, the game crashes. Block the check.
                if (__instance.TargetPlayer == null)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin), "Update")]
    public static class SewerGoblin_Update_Patch
    {
        public static void Postfix(ScheduleOne.NPCs.CharacterClasses.SewerGoblin __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Force-fix null target to prevent crashes
                if (__instance.TargetPlayer == null && __instance.gameObject.activeInHierarchy)
                {
                    var player = SurvivalController.Instance.GetNearestPlayer(__instance.transform.position);
                    if (player != null)
                    {
                        var field = typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin).GetField("<TargetPlayer>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        field?.SetValue(__instance, player);
                    }
                }

                // ONLY force Attacking if alive. 
                if (__instance.gameObject.activeInHierarchy && !__instance.Health.IsDead)
                {
                    var prop = typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin).GetProperty("CurrentState", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null && (ScheduleOne.NPCs.CharacterClasses.SewerGoblin.ESewerGoblinState)prop.GetValue(__instance) != ScheduleOne.NPCs.CharacterClasses.SewerGoblin.ESewerGoblinState.Attacking)
                    {
                        prop.SetValue(__instance, ScheduleOne.NPCs.CharacterClasses.SewerGoblin.ESewerGoblinState.Attacking, null);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin), nameof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin.Retreat))]
    public static class SewerGoblin_Retreat_Patch
    {
        public static bool Prefix()
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Never let the boss run away in survival mode
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.Schedules.NPCEvent_StayInBuilding), "Started")]
    public static class NPCEvent_StayInBuilding_Started_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.Schedules.NPCEvent_StayInBuilding __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var npcField = typeof(ScheduleOne.NPCs.Schedules.NPCAction).GetField("npc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var npc = npcField?.GetValue(__instance) as ScheduleOne.NPCs.NPC;
                
                if (npc is ScheduleOne.NPCs.CharacterClasses.SewerGoblin)
                {
                    return false; // Block the event from starting
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.NPCs.Schedules.NPCEvent_StayInBuilding), "OnActiveTick")]
    public static class NPCEvent_StayInBuilding_OnActiveTick_Patch
    {
        public static bool Prefix(ScheduleOne.NPCs.Schedules.NPCEvent_StayInBuilding __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var npcField = typeof(ScheduleOne.NPCs.Schedules.NPCAction).GetField("npc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var npc = npcField?.GetValue(__instance) as ScheduleOne.NPCs.NPC;

                if (npc is ScheduleOne.NPCs.CharacterClasses.SewerGoblin)
                {
                    return false; // Block the event from ticking
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

    [HarmonyPatch(typeof(ScheduleOne.NPCs.NPCMovement), "UpdateSpeed")]
    public static class NPCMovement_UpdateSpeed_Patch
    {
        public static void Postfix(ScheduleOne.NPCs.NPCMovement __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var npc = __instance.GetComponent<ScheduleOne.NPCs.NPC>();
                if (npc != null)
                {
                    SurvivalController.Instance.ReApplyCurrentModifier(npc);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.UI.Compass.CompassManager), "LateUpdate")]
    public static class CompassManager_LateUpdate_Patch
    {
        public static void Postfix(ScheduleOne.UI.Compass.CompassManager __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Forcibly disable the compass canvas while in survival mode
                if (__instance.Canvas != null) __instance.Canvas.enabled = false;
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Money.MoneyManager), nameof(ScheduleOne.Money.MoneyManager.ChangeCashBalance))]
    public static class MoneyManager_ChangeCashBalance_Patch
    {
        public static bool Prefix(ScheduleOne.Money.MoneyManager __instance, float change, bool visualizeChange, bool playCashSound)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Force all cash changes to be local-only during survival
                if (ScheduleOne.PlayerScripts.Player.Local != null)
                {
                    var inv = ScheduleOne.PlayerScripts.Player.Local.GetComponent<ScheduleOne.PlayerScripts.PlayerInventory>();
                    if (inv != null && inv.cashInstance != null)
                    {
                        // Direct local application
                        inv.cashInstance.ChangeBalance(change);
                        
                        // We still want the UI feedback from the original method, 
                        // but we return false to prevent the game's shared cashInstance from being touched.
                        // For now, simple return false ensures no shared money.
                        return false; 
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.UI.HUD), "Update")]
    public static class HUD_Update_Patch
    {
        public static void Postfix(ScheduleOne.UI.HUD __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Forcibly hide bank/card UI components
                if (__instance.onlineBalanceContainer != null && __instance.onlineBalanceContainer.gameObject.activeSelf) 
                    __instance.onlineBalanceContainer.gameObject.SetActive(false);
                
                if (__instance.onlineBalanceSlotUI != null && __instance.onlineBalanceSlotUI.gameObject.activeSelf) 
                    __instance.onlineBalanceSlotUI.gameObject.SetActive(false);

                // Ensure cash remains visible
                if (__instance.cashSlotContainer != null && !__instance.cashSlotContainer.gameObject.activeSelf)
                    __instance.cashSlotContainer.gameObject.SetActive(true);

                if (__instance.cashSlotUI != null && !__instance.cashSlotUI.gameObject.activeSelf)
                    __instance.cashSlotUI.gameObject.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Vision.VisionCone), "EventFullyNoticed")]
    public static class VisionCone_EventFullyNoticed_Patch
    {
        public static bool Prefix()
        {
            if (SurvivalController.Instance.SurvivalEnabled) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Vision.VisionCone), "UpdateEvents")]
    public static class VisionCone_UpdateEvents_Patch
    {
        public static void Postfix(ScheduleOne.Vision.VisionCone __instance)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                if (__instance.QuestionMarkPopup != null && __instance.QuestionMarkPopup.enabled)
                {
                    __instance.QuestionMarkPopup.enabled = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScheduleOne.Vision.VisionEvent), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(ScheduleOne.Vision.VisionCone), typeof(ScheduleOne.Vision.ISightable), typeof(ScheduleOne.Vision.EntityVisualState), typeof(float), typeof(bool) })]
    public static class VisionEvent_Constructor_Patch
    {
        public static void Prefix(ref bool _playTremolo)
        {
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                // Forcibly silence the tremolo/suspicion sound
                _playTremolo = false;
            }
        }
    }
}
