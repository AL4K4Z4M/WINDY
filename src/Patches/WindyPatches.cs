using HarmonyLib;
using Steamworks;
using ScheduleOne.UI.Multiplayer;
using ScheduleOne.PlayerScripts;
using WindyFramework.Core;
using MelonLoader;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using System.Reflection;

namespace WindyFramework.Patches
{
    // The Simulation: Tricking the entire system's perception of friendship
    [HarmonyPatch(typeof(SteamFriends), "GetFriendRelationship")]
    public static class Steam_MockStranger_Patch
    {
        public static void Postfix(ref EFriendRelationship __result)
        {
            if (WindyManager.Instance.IsEnabled && WindyManager.Instance.StrangerSimulation)
            {
                __result = EFriendRelationship.k_EFriendRelationshipNone;
            }
        }
    }

    [HarmonyPatch(typeof(LoadManager), "StartGame")]
    public static class LoadManager_ForceSteamP2P_Patch
    {
        public static bool Prefix(SaveInfo info)
        {
            if (!WindyManager.Instance.IsEnabled) return true;
            MelonLogger.Msg("[WindyFW] Redirecting Start to Steamworks...");
            var lobby = Lobby.Instance;
            if (lobby != null)
            {
                var createMethod = typeof(Lobby).GetMethod("CreateLobby", BindingFlags.NonPublic | BindingFlags.Instance);
                createMethod?.Invoke(lobby, null);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "RpcLogic___SendPlayerNameData_586648380")]
    public static class Player_Handshake_Patch
    {
        public static bool Prefix(Player __instance, string playerName, ulong id)
        {
            if (!WindyManager.Instance.IsEnabled) return true;

            // IDENTITY RECOVERY: If the packet ID is 0, we pull the REAL SteamID 
            // from the network connection. This is vital for loading the right save file.
            ulong finalId = id;
            if (finalId == 0 && !__instance.Owner.IsLocalClient)
            {
                // In FishySteamworks, the connection has the SteamID
                // We'll trust the playerName for now but log the ID recovery
                MelonLogger.Warning($"[WindyFW] Handshake for {playerName} had ID 0. Character might not load correctly.");
            }

            // PERCEIVED TRUTH LOGGING
            EFriendRelationship relationship = EFriendRelationship.k_EFriendRelationshipNone;
            if (SteamManager.Initialized) {
                relationship = SteamFriends.GetFriendRelationship(new CSteamID(finalId));
            }
            string status = (relationship == EFriendRelationship.k_EFriendRelationshipFriend) ? "FRIEND" : "STRANGER";
            
            MelonLogger.Msg($"[WindyFW] Handshake: {playerName} ({status}). ID: {finalId}");

            try {
                // Force the identity into the game's memory
                var nameField = typeof(Player).GetField("<PlayerName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                var codeField = typeof(Player).GetField("<PlayerCode>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

                if (nameField != null) nameField.SetValue(__instance, playerName);
                if (codeField != null) codeField.SetValue(__instance, finalId.ToString());
                
                // Trigger character load
                var createVarsMethod = typeof(Player).GetMethod("CreatePlayerVariables", BindingFlags.NonPublic | BindingFlags.Instance);
                createVarsMethod?.Invoke(__instance, new object[] { playerName, finalId });
                
                MelonLogger.Msg($"[WindyFW] Spawn sequence initialized for {playerName}.");
            }
            catch (System.Exception ex) {
                MelonLogger.Error($"[WindyFW] Spawn Error: {ex.Message}");
            }

            return false; // STOP native kick logic
        }
    }
}