using UnityEngine;
using MelonLoader;
using System.Collections.Generic;
using ScheduleOne.UI;
using ScheduleOne.PlayerScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Cartel;

namespace Zordon.ScheduleI.Survival.Features
{
    // Pure C# Class - No MonoBehaviour
    public class DebugMenu
    {
        private static DebugMenu _instance;
        public static DebugMenu Instance => _instance ?? (_instance = new DebugMenu());

        public bool IsOpen { get; private set; } = false; // Disable Force Open
        private Rect _windowRect = new Rect(20, 20, 400, 600);
        public bool GodModeEnabled { get; private set; } = false;
        public static bool FreezeNPCVision = false;
        
        private int _currentTab = 0;
        private string[] _tabs = { "State", "Wave", "Entities", "Cheats", "Drug Lab", "Debug" };
        private string _waveInput = "1";
        private Vector2 _drugScroll = Vector2.zero;
        
        private float _logTimer = 0f;

        public void OnUpdate()
        {
            _logTimer += Time.deltaTime;
            if(_logTimer > 5f)
            {
                _logTimer = 0f;
                // MelonLogger.Msg("[DebugMenu] Heartbeat..."); // Uncomment if desperate
            }

            if (Input.GetKeyDown(KeyCode.F3)) 
            {
                IsOpen = !IsOpen;
                MelonLogger.Msg($"[DebugMenu] Input Detected! Open: {IsOpen}");
                
                if(IsOpen)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            if(GodModeEnabled && Player.Local != null)
            {
                Player.Local.Health.SetHealth(100f);
            }
        }

        public void OnGUI()
        {
            if (!IsOpen) return;

            // Draw Window
            _windowRect = GUI.Window(1001, _windowRect, (GUI.WindowFunction)DrawWindow, "SURVIVAL COMMANDER (F3/INS)");
        }

        private void DrawWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 400, 20));

            GUILayout.BeginVertical();
            GUILayout.Space(10);

            // Tabs
            GUILayout.BeginHorizontal();
            for(int i=0; i<_tabs.Length; i++)
            {
                GUI.backgroundColor = _currentTab == i ? Color.cyan : Color.white;
                if(GUILayout.Button(_tabs[i])) _currentTab = i;
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);

            // Content
            switch(_currentTab)
            {
                case 0: DrawStateControl(); break;
                case 1: DrawWaveControl(); break;
                case 2: DrawEntityControl(); break;
                case 3: DrawCheats(); break;
                case 4: DrawDrugLab(); break;
                case 5: DrawDiagnostics(); break;
            }

            GUILayout.EndVertical();
        }

        // 1. STATE CONTROL
        private void DrawStateControl()
        {
            GUILayout.Label("<b>GAME STATE</b>", GetLabelStyle(Color.yellow));
            
            string status = SurvivalController.Instance.SurvivalEnabled ? "<color=green>ACTIVE</color>" : "<color=red>DISABLED</color>";
            if(SurvivalController.Instance.IsWaveActive) status += " (IN WAVE)";
            else if(SurvivalController.Instance.IsLostState) status += " (LOST)";
            
            GUILayout.Label($"Status: {status}");
            GUILayout.Space(5);

            if (GUILayout.Button(SurvivalController.Instance.SurvivalEnabled ? "DISABLE SURVIVAL" : "ENABLE SURVIVAL"))
            {
                SurvivalController.Instance.SurvivalEnabled = !SurvivalController.Instance.SurvivalEnabled;
                if(!SurvivalController.Instance.SurvivalEnabled) SurvivalController.Instance.StopWaves();
            }

            if (GUILayout.Button("FORCE START WAVE 1"))
            {
                SurvivalController.Instance.SurvivalEnabled = true;
                SurvivalController.Instance.StartNewWave();
            }

            if (GUILayout.Button("FORCE END (CLEANUP)"))
            {
                SurvivalController.Instance.StopWaves();
            }

            if (GUILayout.Button("FORCE LOSS (TEST DEATH)"))
            {
                SurvivalController.Instance.OnPlayerLost();
            }
        }

        // 2. WAVE MANIPULATION
        private void DrawWaveControl()
        {
            GUILayout.Label("<b>WAVE TIME MACHINE</b>", GetLabelStyle(Color.yellow));
            GUILayout.Label($"Current Wave: {SurvivalController.Instance.CurrentWave}");

            if (GUILayout.Button("SKIP WAVE (KILL ALL)"))
            {
                SurvivalController.Instance.ForceSkipWave();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Wave:");
            _waveInput = GUILayout.TextField(_waveInput, GUILayout.Width(50));
            // Placeholder for set wave logic
            GUILayout.EndHorizontal();
        }

        // 3. ENTITY MANAGEMENT
        private void DrawEntityControl()
        {
            GUILayout.Label("<b>SPAWNER</b>", GetLabelStyle(Color.yellow));

            if (GUILayout.Button("SPAWN ARMORY VAN (HERE)"))
            {
                SpawnVanAtPlayer();
            }

            if (GUILayout.Button("SPAWN GOON (HERE)"))
            {
                SpawnGoonAtPlayer();
            }

            if (GUILayout.Button("SPAWN COP (HERE)"))
            {
                SpawnCopAtPlayer();
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>CLEANUP</b>", GetLabelStyle(Color.red));

            if (GUILayout.Button("KILL ALL (SKIP WAVE)"))
            {
                SurvivalController.Instance.ForceSkipWave();
            }
        }

        // 4. PLAYER CHEATS
        private void DrawCheats()
        {
            GUILayout.Label("<b>POWER FANTASY</b>", GetLabelStyle(Color.yellow));

            if (GUILayout.Button(GodModeEnabled ? "GOD MODE: <color=green>ON</color>" : "GOD MODE: <color=red>OFF</color>"))
            {
                GodModeEnabled = !GodModeEnabled;
            }

            if (GUILayout.Button(FreezeNPCVision ? "FREEZE NPC VISION: <color=green>ON</color>" : "FREEZE NPC VISION: <color=red>OFF</color>"))
            {
                FreezeNPCVision = !FreezeNPCVision;
            }

            if (GUILayout.Button("ADD $10,000"))
            {
                if(NetworkSingleton<MoneyManager>.Instance != null) 
                    NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(10000f);
            }

            if (GUILayout.Button("SUICIDE"))
            {
                if(Player.Local != null) Player.Local.Health.Die(); 
            }
            
            if (GUILayout.Button("FORCE REVIVE (UNSTUCK)"))
            {
                ForceRevive();
            }
        }

        // 5. DRUG LAB
        private void DrawDrugLab()
        {
            GUILayout.Label("<b>THE DRUG LAB</b>", GetLabelStyle(Color.magenta));
            GUILayout.Label("Experiment with all available game effects.");
            GUILayout.Space(5);

            if (GUILayout.Button("GIVE RANDOM DRUG EFFECT"))
            {
                DrugDebugModule.Instance.GiveRandomEffect();
            }

            if (GUILayout.Button("CLEAR ACTIVE EFFECT"))
            {
                DrugDebugModule.Instance.ClearActiveEffect();
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>MANUAL SELECTION</b>", GetLabelStyle(Color.cyan));
            
            _drugScroll = GUILayout.BeginScrollView(_drugScroll, GUILayout.Height(350));
            var allEffects = DrugDebugModule.Instance.GetAllEffects();
            foreach (var effect in allEffects)
            {
                if (GUILayout.Button(effect.Name))
                {
                    DrugDebugModule.Instance.ApplyEffect(effect);
                }
            }
            GUILayout.EndScrollView();
        }

        // 6. DIAGNOSTICS (RENAMED TO SPAWN MANAGER)
        private void DrawDiagnostics()
        {
            GUILayout.Label("<b>VISUALS</b>", GetLabelStyle(Color.cyan));

            if (GUILayout.Button(DebugLabel.ShowLabels ? "NAMES: <color=green>VISIBLE</color>" : "NAMES: <color=red>HIDDEN</color>"))
            {
                DebugLabel.ShowLabels = !DebugLabel.ShowLabels;
            }

            if (GUILayout.Button(DebugLabel.ShowBeams ? "BEAMS: <color=green>VISIBLE</color>" : "BEAMS: <color=red>HIDDEN</color>"))
            {
                DebugLabel.ShowBeams = !DebugLabel.ShowBeams;
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>SPAWN MANAGER</b>", GetLabelStyle(Color.yellow));

            if (GUILayout.Button("ADD PLAYER START POINT (BLUE)"))
            {
                LocationLogger.Instance.LogLocation("Survival_PlayerSpawnPoints.txt", Color.cyan);
            }

            if (GUILayout.Button("ADD ENEMY SPAWN (RED)"))
            {
                LocationLogger.Instance.LogLocation("Survival_EnemySpawnPoints.txt", Color.red);
            }

            if (GUILayout.Button("ADD VAN SPAWN (GREEN)"))
            {
                LocationLogger.Instance.LogLocation("Survival_SpawnPoints.txt", Color.green);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("TOGGLE ALL MARKERS"))
            {
                LocationLogger.Instance.ToggleMarkers();
            }
        }

        private GUIStyle GetLabelStyle(Color col)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.normal.textColor = col;
            return style;
        }

        private void KillAllEnemies()
        {
            var goons = GameObject.FindObjectsOfType<CartelGoon>();
            foreach(var goon in goons)
            {
                if(goon.Health != null && !goon.Health.IsDead)
                {
                    goon.Health.TakeDamage(9999f);
                }
            }
        }

        private void SpawnVanAtPlayer()
        {
            if(Player.Local == null) return;
            var pos = Player.Local.transform.position + (Player.Local.transform.forward * 5f) + (Vector3.up * 2f);
            var rot = Player.Local.transform.rotation;
            ScheduleOne.Vehicles.VehicleManager.Instance.SpawnAndReturnVehicle("Veeper", pos, rot, false);
        }

        private void SpawnGoonAtPlayer()
        {
            var pool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.GoonPool;
            if(pool != null && Player.Local != null)
            {
                var pos = Player.Local.transform.position + (Player.Local.transform.forward * 5f);
                var goon = pool.SpawnGoon(pos);
                if(goon != null)
                {
                    goon.Health.Revive();
                    goon.AttackEntity(Player.Local);
                }
            }
        }

        private void SpawnCopAtPlayer()
        {
            if (Player.Local == null) return;
            var station = ScheduleOne.Map.PoliceStation.GetClosestPoliceStation(Player.Local.transform.position);
            if(station != null && station.OfficerPool.Count > 0)
            {
                var cop = station.PullOfficer();
                if(cop != null)
                {
                    var pos = Player.Local.transform.position + (Player.Local.transform.forward * 5f);
                    cop.Movement.Warp(pos);
                    cop.BeginFootPursuit_Networked(Player.Local.PlayerCode, false);
                    MelonLogger.Msg("Spawned Cop!");
                }
                else
                {
                    MelonLogger.Warning("Failed to pull officer (Null)");
                }
            }
            else
            {
                MelonLogger.Warning("No Officers in pool!");
            }
        }

        private void ForceRevive()
        {
            if(Player.Local != null) {
                Player.Local.Health.SetHealth(100f);
                Player.Local.SetRagdolled(false);
                PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
                PlayerSingleton<PlayerCamera>.Instance.SetCanLook(true);
                PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
                PlayerSingleton<PlayerInventory>.Instance.SetViewmodelVisible(true);
                if (Singleton<HUD>.InstanceExists) Singleton<HUD>.Instance.canvas.enabled = true;
            }
        }
    }
}
