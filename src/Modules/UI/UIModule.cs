using MelonLoader;
using UnityEngine;
using WindyFramework.Core;
using WindyFramework.Modules.UI.Patches;
using Steamworks;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using System.Collections;

namespace WindyFramework.Modules.UI
{
    public class UIModule : IWindyModule
    {
        public static UIModule Instance { get; private set; }

        private bool _steamCallbacksRegistered = false;
        private Callback<LobbyMatchList_t> _lobbyMatchListCallback;
        private Callback<P2PSessionRequest_t> _p2pRequestCallback;

        public void OnInitialize()
        {
            Instance = this;
            MelonLogger.Msg("[UIModule] Initialized.");
        }

        public void OnUpdate()
        {
            if (!_steamCallbacksRegistered && SteamManager.Initialized)
            {
                // Register Discovery Callback
                _lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(WindyManager.Instance.OnLobbyMatchList);
                
                // Register the "Doorman" (P2P Acceptance)
                _p2pRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PRequest);
                
                _steamCallbacksRegistered = true;
                MelonLogger.Msg("[WindyFW] Steam Doorman is now on duty.");
            }

            WindyManager.Instance.OnUpdate();
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnGUI()
        {
            WindyManager.Instance.OnGUI();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                // Hook into load complete to ensure Money/Biz data is ready
                if (LoadManager.Instance != null)
                {
                    LoadManager.Instance.onLoadComplete.AddListener(new UnityEngine.Events.UnityAction(() => {
                        MelonCoroutines.Start(DelayedMetadataUpdate());
                    }));
                }
                
                // Inject Phone App
                WindyFramework.Modules.UI.Features.PhoneInjector.TryInject();
            }
            else if (sceneName == "Menu")
            {
                MenuInjector.Inject();
            }
        }

        public void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
        }

        public void OnPreferencesSaved()
        {
        }

        private void OnP2PRequest(P2PSessionRequest_t result)
        {
            // SECURITY: Sanitize logs to prevent leaks
            string safeId = SanitizeID(result.m_steamIDRemote);

            // SECURITY: Context Check
            // Only accept P2P connections if we are actually inside a Lobby (Host or Client).
            // This prevents random IP scanners from initiating handshakes while we are in the Main Menu.
            bool inLobby = Lobby.Instance != null && Lobby.Instance.IsInLobby;

            if (inLobby)
            {
                MelonLogger.Msg($"[WindyFW] Doorman: Accepting incoming connection from {safeId}");
                SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
            }
            else
            {
                MelonLogger.Warning($"[WindyFW] Doorman: BLOCKED unsolicited P2P request from {safeId} (Not in Lobby)");
                // We do NOT call AcceptP2PSessionWithUser here.
            }
        }

        private string SanitizeID(CSteamID id)
        {
            string s = id.ToString();
            if (s.Length > 8) return s.Substring(0, 4) + "***" + s.Substring(s.Length - 4);
            return "REDACTED";
        }

        private IEnumerator DelayedMetadataUpdate()
        {
            yield return new WaitForSeconds(2f); // Wait 2s for MoneyManager to calculate net worth
            WindyManager.Instance.ForceMetadataUpdate();
        }
    }
}
