using MelonLoader;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using ScheduleOne.UI.Multiplayer;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using FishNet.Transporting;
using ScheduleOne.Money;
using System;

namespace WindyFramework.Core
{
    public class WindyManager
    {
        private static WindyManager _instance;
        public static WindyManager Instance => _instance ?? (_instance = new WindyManager());

        public const string VERSION_WINDYFW = "1.2.3";
        public bool IsEnabled = true;
        public bool ShowUI = false;
        public bool StrangerSimulation = false; 

        public List<LobbyData> FoundLobbies = new List<LobbyData>();
        private float _lastDiscoveryTime = 0f;
        private float _lastMetadataUpdateTime = 0f;
        private Rect _windowRect = new Rect(Screen.width - 420, 50, 450, 550);
        private Rect _hostWindowRect = new Rect(50, 50, 300, 400);

        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private bool _isJoining = false;

        public struct LobbyData
        {
            public CSteamID Id;
            public string HostName;
            public string Version;
            public string BusinessName;
            public string NetWorth;
            public string GameMode;
        }

        public Action OnLobbiesUpdated;

        public void OnUpdate()
        {
            if (!IsEnabled) return;

            if (Input.GetKeyDown(KeyCode.F4))
            {
                ShowUI = !ShowUI;
                // Only refresh if we opened the menu while in the Main Menu scene
                if (ShowUI && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu") 
                    RefreshLobbyList();
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu")
            {
                if (Time.time - _lastDiscoveryTime > 10f) RefreshLobbyList();
            }

            // Periodic Host Metadata Update (Every 5 seconds)
            if (Lobby.Instance != null && Lobby.Instance.IsHost && Time.time - _lastMetadataUpdateTime > 5f)
            {
                ForceMetadataUpdate();
                _lastMetadataUpdateTime = Time.time;
            }
        }

        public void RefreshLobbyList()
        {
            if (!SteamAPI.IsSteamRunning()) return;
            _lastDiscoveryTime = Time.time;
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();
        }

        public void OnLobbyMatchList(LobbyMatchList_t callback)
        {
            FoundLobbies.Clear();
            for (int i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                if (SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_ACTIVE") == "true")
                {
                    FoundLobbies.Add(new LobbyData {
                        Id = lobbyId,
                        HostName = SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_HOST"),
                        Version = SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_VER"),
                        BusinessName = SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_BIZ"),
                        NetWorth = SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_NETWORTH"),
                        GameMode = SteamMatchmaking.GetLobbyData(lobbyId, "WINDY_MODE")
                    });
                }
            }
            OnLobbiesUpdated?.Invoke();
        }

        public void ForceMetadataUpdate()
        {
            if (!SteamAPI.IsSteamRunning()) return;
            var lobby = Lobby.Instance;
            if (lobby != null && lobby.LobbySteamID != CSteamID.Nil)
            {
                CSteamID lobbyId = lobby.LobbySteamID;
                SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_ACTIVE", "true");
                SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_VER", VERSION_WINDYFW);
                SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_HOST", SteamFriends.GetPersonaName());
                SteamMatchmaking.SetLobbyType(lobbyId, ELobbyType.k_ELobbyTypePublic);

                // Extended Metadata
                if (LoadManager.Instance != null && LoadManager.Instance.ActiveSaveInfo != null)
                {
                    SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_BIZ", LoadManager.Instance.ActiveSaveInfo.OrganisationName);
                }
                
                if (MoneyManager.Instance != null)
                {
                    float net = MoneyManager.Instance.GetNetWorth();
                    SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_NETWORTH", MoneyManager.FormatAmount(net));
                }

                SteamMatchmaking.SetLobbyData(lobbyId, "WINDY_MODE", "Freemode");

                MelonLogger.Msg($"[WindyFW] Shouting v{VERSION_WINDYFW} to the world.");
            }
        }

        public void JoinLobbyDirect(LobbyData data)
        {
            if (_isJoining) return;
            _isJoining = true;
            _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            SteamMatchmaking.JoinLobby(data.Id);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            _isJoining = false;
            if (callback.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);
                LoadManager.Instance.LoadAsClient(hostId.m_SteamID.ToString());
                ShowUI = false; // Auto-hide UI on success
            }
            if (_lobbyEnterCallback != null) { _lobbyEnterCallback.Dispose(); _lobbyEnterCallback = null; }
        }

        public void OnGUI()
        {
            if (!IsEnabled || !ShowUI) return;

            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isMenu = scene == "Menu";
            bool isHost = Lobby.Instance != null && Lobby.Instance.IsHost;

            if (isMenu)
            {
                // Main Menu: Show Browser
                _windowRect = GUI.Window(999, _windowRect, DrawBrowser, $"WindyFramework v{VERSION_WINDYFW}");
            }
            else if (isHost)
            {
                // In-Game Host: Show Control Panel
                _hostWindowRect = GUI.Window(1000, _hostWindowRect, DrawHostPanel, "Host Control");
            }
            // In-Game Client: Show Nothing (ShowUI does nothing essentially)
        }

        private void DrawHostPanel(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label($"<b>Connected Players: {Player.PlayerList.Count}</b>");
            
            foreach (var p in Player.PlayerList)
            {
                if (p == null) continue;
                
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical();
                GUILayout.Label($"<b>{p.PlayerName}</b>");
                GUILayout.Label($"<size=10>ID: {p.PlayerCode}</size>");
                GUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();

                if (p.IsLocalPlayer)
                {
                    GUILayout.Label("YOU");
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("KICK", GUILayout.Width(50)))
                    {
                        KickPlayer(p);
                    }
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void SetLobbyVisibility(bool isPublic)
        {
            if (!SteamAPI.IsSteamRunning() || Lobby.Instance == null || !Lobby.Instance.IsHost) return;
            
            CSteamID lobbyId = Lobby.Instance.LobbySteamID;
            ELobbyType type = isPublic ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePrivate;
            SteamMatchmaking.SetLobbyType(lobbyId, type);
            MelonLogger.Msg($"[WindyFW] Set Lobby Visibility to: {type}");
        }

        public void KickPlayer(Player p)
        {
            if (p == null) return;
            MelonLogger.Msg($"[WindyFW] Kicking player {p.PlayerName} ({p.PlayerCode})...");

            try
            {
                // 1. Close P2P Session (The Hard Kick)
                if (ulong.TryParse(p.PlayerCode, out ulong steamId))
                {
                    CSteamID cSteamId = new CSteamID(steamId);
                    SteamNetworking.CloseP2PSessionWithUser(cSteamId);
                    MelonLogger.Msg($"[WindyFW] Closed P2P session with {steamId}");
                }

                // 2. Disconnect from FishNet (The Soft Kick)
                if (p.Connection != null)
                {
                    p.Connection.Disconnect(false); // false = immediate
                    MelonLogger.Msg($"[WindyFW] Disconnected FishNet connection.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WindyFW] Failed to kick player: {ex.Message}");
            }
        }

        private void DrawBrowser(int id)
        {
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal("box");
            StrangerSimulation = GUILayout.Toggle(StrangerSimulation, " MOCK STRANGER TEST");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Refresh List")) RefreshLobbyList();
            GUILayout.Space(10);

            if (_isJoining) GUILayout.Label("Steam Handshake...");
            else if (FoundLobbies.Count == 0) GUILayout.Label("No modded lobbies found...");

            foreach (var lobby in FoundLobbies)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical();
                GUILayout.Label($"<b>{lobby.HostName}</b>");
                GUILayout.Label($"Ver: {lobby.Version}");
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("JOIN")) JoinLobbyDirect(lobby);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
