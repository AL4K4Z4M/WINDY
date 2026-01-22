using HarmonyLib;
using ScheduleOne.Intro;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Clothing;
using System.Collections.Generic;
using MelonLoader;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Persistence;
using ScheduleOne.DevUtilities;
using Zordon.ScheduleI.Survival.Features;
using UnityEngine;
using ScheduleOne.GameTime;

namespace Zordon.ScheduleI.Survival.Patches
{
    [HarmonyPatch(typeof(IntroManager), "Play")]
    public static class IntroManager_Play_Patch
    {
        public static bool Prefix(IntroManager __instance)
        {
            if (SurvivalController.Instance != null && SurvivalController.Instance.IsSurvivalPending)
            {
                MelonLogger.Msg("[Survival] Intercepting Intro Play. Skipping Animation.");
                Singleton<CoroutineService>.Instance.StartCoroutine(InstantPlay(__instance));
                return false;
            }
            return true;
        }

        private static System.Collections.IEnumerator InstantPlay(IntroManager intro)
        {
            MelonLogger.Msg("[Survival] InstantPlay Routine Started.");

            // Do NOT activate intro.Container (this holds the cutscene visuals/animation)
            // If the Animation plays automatically on the GameObject, we must stop it or keep it inactive.
            if (intro.Anim != null) intro.Anim.Stop();

            // Minimal Setup to lock player while loading
            PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
            Singleton<HUD>.Instance.canvas.enabled = false;
            PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
            
            // Lock Mouse
            PlayerSingleton<PlayerCamera>.Instance.LockMouse();

            // Wait for Game Loaded
            yield return new WaitUntil(() => Singleton<LoadManager>.Instance.IsGameLoaded);
            MelonLogger.Msg("[Survival] Game Loaded. Opening Character Creator.");

            // Directly Open Character Creator
            yield return new WaitForSeconds(0.1f);
            
            Singleton<CharacterCreator>.Instance.Open(Singleton<CharacterCreator>.Instance.DefaultSettings);
            Singleton<CharacterCreator>.Instance.onCompleteWithClothing.AddListener(intro.CharacterCreationDone);
        }
    }

    [HarmonyPatch(typeof(IntroManager), "CharacterCreationDone")]
    public static class IntroManager_CharacterCreationDone_Patch
    {
        public static bool Prefix(BasicAvatarSettings avatar, List<ClothingInstance> clothes)
        {
            // CHECK IsSurvivalPending instead of SurvivalEnabled
            if (SurvivalController.Instance != null && SurvivalController.Instance.IsSurvivalPending)
            {
                MelonLogger.Msg("[Survival] Intercepting Character Creation Completion.");
                
                // Activate Survival Mode
                SurvivalController.Instance.SurvivalEnabled = true;
                SurvivalController.Instance.IsSurvivalPending = false;

                // Custom Survival Start Logic
                Singleton<CoroutineService>.Instance.StartCoroutine(SurvivalStartRoutine(clothes));
                
                return false; // Skip original
            }
            return true;
        }

        private static System.Collections.IEnumerator SurvivalStartRoutine(List<ClothingInstance> clothes)
        {
            // Wait a bit
            yield return new WaitForSeconds(0.5f);

            // Hide Character Creator
            Singleton<CharacterCreator>.Instance.DisableStuff();
            
            // Fix Camera & Controls
            PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, false);
            PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
            
            // IntroManager usually adds itself as UI element, limiting mouse. Remove it.
            if (IntroManager.Instance != null)
            {
                PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(IntroManager.Instance.name);
            }
            
            PlayerSingleton<PlayerCamera>.Instance.SetCanLook(true);
            PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
            PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(true);
            
            // Enable HUD
            Singleton<HUD>.Instance.canvas.enabled = true;
            Singleton<BlackOverlay>.Instance.Close(1f);

            // Give Clothes
            foreach (var c in clothes)
            {
                Player.Local.Clothing.InsertClothing(c);
            }

            // Teleport to Spawn Point
            Vector3 customSpawn = SurvivalLaunch.GetRandomPlayerSpawn();
            if (customSpawn != Vector3.zero)
            {
                 PlayerSingleton<PlayerMovement>.Instance.Teleport(customSpawn);
                 MelonLogger.Msg($"[Survival] Warped to custom spawn: {customSpawn}");
            }
            else if (NetworkSingleton<GameManager>.Instance != null)
            {
                 PlayerSingleton<PlayerMovement>.Instance.Teleport(NetworkSingleton<GameManager>.Instance.SpawnPoint.position);
            }

            // Trigger Survival Logic
            MelonLogger.Msg("[Survival] Starting First Wave Sequence...");
            SurvivalController.Instance.StartNewWave();

            // Save
            if (FishNet.InstanceFinder.IsServer)
            {
                Singleton<SaveManager>.Instance.Save();
            }
        }
    }
}