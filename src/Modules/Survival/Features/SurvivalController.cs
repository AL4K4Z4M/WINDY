using UnityEngine;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Cartel;
using ScheduleOne.PlayerScripts;
using ScheduleOne.DevUtilities;
using ScheduleOne.Combat;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI;
using ScheduleOne.Money;
using UnityEngine.AI;
using ScheduleOne.Police;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.Effects;
using System.IO;
using FishNet;
using System.Reflection;

namespace Zordon.ScheduleI.Survival.Features
{
    public class SurvivalController : MonoBehaviour
    {
        private static SurvivalController _instance;
        public static SurvivalController Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SurvivalController");
                    Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<SurvivalController>();
                }
                return _instance;
            }
        }

        private WaveManager _waveManager = new WaveManager();
        public int CurrentWave => _waveManager.CurrentWave;

        public bool IsWaveActive { get; private set; } = false;
        public bool SurvivalEnabled { get; set; } = false;
        public bool IsSurvivalPending { get; set; } = false;
        
        private bool _isLost = false;
        public bool IsLostState => _isLost;

        private bool _isWaitingForNextWave = false;
        
        private const int MAX_ACTIVE_GOONS = 50;
        private const int MAX_ACTIVE_COPS = 50;

        private List<CartelGoon> _activeGoons = new List<CartelGoon>();
        private List<PoliceOfficer> _activeCops = new List<PoliceOfficer>();
        private List<NPC> _activeCivilians = new List<NPC>();
        private List<NPC> _activeBosses = new List<NPC>();
        private List<ScheduleOne.Vehicles.LandVehicle> _activeVans = new List<ScheduleOne.Vehicles.LandVehicle>();
        
        private class SpawnPoint
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float LastUsedTime;
        }
        private List<SpawnPoint> _vanSpawnPoints = new List<SpawnPoint>();
        private List<SpawnPoint> _enemySpawnPoints = new List<SpawnPoint>();

        private struct SavedItem { public int Index; public ItemInstance Instance; }
        private List<SavedItem> _savedInventory = new List<SavedItem>();
        private float _savedCash = 0f;
        private float _savedBankBalance = 0f;

        private GUIStyle _waveStyle;
        private float _spawnTimer = 0f;
        private float _nextWaveTimeRemaining = 0f;
        private float _hudAlpha = 0f;
        private float _bossHudAlpha = 0f;
        private int _cachedAliveCount = 0;
        private bool _isAwaitingTransition = false;

        private bool _isInitialPrep = false;
        private float _initialPrepTimeRemaining = 0f;

        private void ClearWorldOfNPCs()
        {
            MelonLogger.Msg("[Survival] Purging the world of all non-survival entities...");

            try 
            {
                // 1. Clear Goons
                var goonPool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance?.GoonPool;
                if (goonPool != null)
                {
                    // Use standard FindObjects since we only care about active/spawned goons here
                    var allGoons = GameObject.FindObjectsOfType<CartelGoon>();
                    foreach (var g in allGoons)
                    {
                        if (g != null)
                        {
                            goonPool.ReturnToPool(g);
                            g.Despawn();
                        }
                    }
                }

                // 2. Clear Cops (SAFE VERSION - Avoid .Deactivate() as it crashes during purge)
                // We use FindObjectsOfTypeAll to find those persistent patrolling ones even if pooled
                var allCops = Resources.FindObjectsOfTypeAll<PoliceOfficer>();
                foreach (var cop in allCops)
                {
                    if (cop != null) 
                    {
                        // PROTECT BOSSES
                        if (cop.GetComponent<DebugLabel>() != null) continue;

                        // Stop any active pursuit logic
                        if (cop.Health != null && !cop.Health.IsDead) cop.gameObject.SetActive(false);
                    }
                }

                // Wipe Station Pools to prevent station-led respawns
                foreach (var station in PoliceStation.PoliceStations)
                {
                    if (station != null) station.OfficerPool.Clear();
                }

                // 3. Deactivate all ambient NPCs registry-wide
                foreach (var npc in NPCManager.NPCRegistry)
                {
                    if (npc != null)
                    {
                        // PROTECT BOSSES: If it has a DebugLabel, it belongs to the active wave!
                        if (npc.GetComponent<DebugLabel>() != null) continue;

                        npc.gameObject.SetActive(false);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[Survival] Purge Error (Non-Fatal): {ex.Message}");
            }
            
            ClearVans();
        }

        public string DataPath 
        { 
            get 
            {
                string path = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "WindyFW");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        private void LoadAllSpawnPoints()
        {
            _vanSpawnPoints.Clear();
            _enemySpawnPoints.Clear();
            LoadSpawnList("Survival_SpawnPoints.txt", _vanSpawnPoints);
            LoadSpawnList("Survival_EnemySpawnPoints.txt", _enemySpawnPoints);
        }

        private void LoadSpawnList(string filename, List<SpawnPoint> list)
        {
            string path = Path.Combine(DataPath, filename);
            if (!File.Exists(path)) return;
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    // Format: Name | x, y, z | rotx, roty, rotz
                    string[] parts = line.Split('|');
                    if (parts.Length < 3) continue;
                    
                    Vector3 pos = ParseVector(parts[1]);
                    Vector3 rot = ParseVector(parts[2]);
                    
                    list.Add(new SpawnPoint { 
                        Position = pos, 
                        Rotation = Quaternion.Euler(rot),
                        LastUsedTime = -100f
                    });
                }
            } catch {}
        }

        private Vector3 ParseVector(string s) { 
            string[] p = s.Trim().Split(','); 
            if(p.Length < 3) return Vector3.zero;
            return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2])); 
        }

        private void StartArmingPhase()
        {
            if (Player.Local == null) return;
            LoadAllSpawnPoints();
            if (_vanSpawnPoints.Count == 0) return;
            string[] weaponIDs = { "baseballbat", "machete", "m1911", "revolver" };
            int vanCount = Mathf.Clamp(_vanSpawnPoints.Count / 3, 2, 10);
            for (int i = 0; i < _vanSpawnPoints.Count; i++) { var temp = _vanSpawnPoints[i]; int r = Random.Range(i, _vanSpawnPoints.Count); _vanSpawnPoints[i] = _vanSpawnPoints[r]; _vanSpawnPoints[r] = temp; }
            for (int i = 0; i < vanCount; i++) {
                var p = _vanSpawnPoints[i];
                // Use transform direction directly for offset
                Vector3 backward = p.Rotation * Vector3.back;
                Vector3 spawnPos = p.Position + (backward * 2f) + Vector3.up;
                var van = ScheduleOne.Vehicles.VehicleManager.Instance.SpawnAndReturnVehicle("Veeper", spawnPos, p.Rotation, false);
                if (van) { InitializeArmoryVan(van, weaponIDs); _activeVans.Add(van); p.LastUsedTime = Time.time + 9999f; }
            }
        }

        private void InitializeArmoryVan(ScheduleOne.Vehicles.LandVehicle van, string[] weapons)
        {
            var storage = van.GetComponent<StorageEntity>();
            if (!storage) return;
            
            storage.AccessSettings = StorageEntity.EAccessSettings.Full;
            storage.SlotCount = 10;
            
            // Clear any existing slots first
            storage.ItemSlots.Clear();
            for (int i = 0; i < 10; i++) 
            { 
                var slot = new ItemSlot(false); 
                slot.SetSlotOwner(storage); 
                storage.ItemSlots.Add(slot); 
            }

            // Insert random weapons
            for (int i = 0; i < 6; i++) 
            { 
                ItemDefinition def = ScheduleOne.Registry.GetItem(weapons[Random.Range(0, weapons.Length)]); 
                if (def != null) 
                {
                    // Use InsertItem which handles the networking and slot finding properly
                    storage.InsertItem(def.GetDefaultInstance());
                } 
            }
        }

        public Player GetNearestPlayer(Vector3 position)
        {
            Player nearest = Player.Local;
            float minDist = float.MaxValue;
            
            foreach (var p in Player.PlayerList)
            {
                if (p == null || !p.Health.IsAlive) continue;
                float d = Vector3.Distance(position, p.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = p;
                }
            }
            return nearest;
        }

        private List<Vector3> GetValidSpawnPoints(int count)
        {
            List<Vector3> results = new List<Vector3>();
            if (Player.Local == null) return results;
            if (_enemySpawnPoints.Count == 0)
            {
                MelonLogger.Error("[Survival] NO ENEMY SPAWN POINTS FOUND! Add points to UserData/WindyFW/Survival_EnemySpawnPoints.txt using F6.");
                return results;
            }

            // 1. Pick a random active player to spawn around
            var allPlayers = Player.PlayerList.FindAll(p => p != null && p.Health != null && p.Health.IsAlive);
            if (allPlayers.Count == 0) return results;
            var targetPlayer = allPlayers[Random.Range(0, allPlayers.Count)];

            // 2. Collect candidates for THIS player
            // Randomize the order of spawn points to avoid picking the same ones first
            List<SpawnPoint> shuffledPoints = new List<SpawnPoint>(_enemySpawnPoints);
            for (int i = 0; i < shuffledPoints.Count; i++)
            {
                var temp = shuffledPoints[i];
                int r = Random.Range(i, shuffledPoints.Count);
                shuffledPoints[i] = shuffledPoints[r];
                shuffledPoints[r] = temp;
            }

            // Filter by distance (35m - 80m)
            List<Vector3> candidates = new List<Vector3>();
            foreach (var p in shuffledPoints)
            {
                float d = Vector3.Distance(p.Position, targetPlayer.transform.position);
                if (d >= 35f && d <= 80f)
                {
                    candidates.Add(p.Position);
                }
            }

            MelonLogger.Msg($"[Survival] Found {candidates.Count} candidates for player {targetPlayer.PlayerName} (Need {count})");

            // 3. Select points (Light Validation)
            // V8.2: We trust your markers. We only check if they are actually on or near NavMesh.
            // We SKIP the strict pathfinding check which often fails in dense urban areas.
            foreach (var pos in candidates)
            {
                if (results.Count >= count) break;

                if (UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit hit, 10.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    results.Add(hit.position);
                }
            }
            
            return results;
        }

        private Vector3 GetValidSpawnPosition()
        {
            var points = GetValidSpawnPoints(1);
            if (points.Count > 0) return points[0] + Vector3.up * 0.5f; // Apply safety offset here too
            return Vector3.zero;
        }
        private void SaveCurrentLocationAsSpawnPoint()
        {
            if (Player.Local == null) return;
            
            LocationLogger.Instance.LogLocation("Survival_EnemySpawnPoints.txt", Color.red);
            
            // Reload to update local cache
            LoadAllSpawnPoints();
            MelonLogger.Msg($"[Survival] Spawn points reloaded. Enemies: {_enemySpawnPoints.Count}");
        }

        [System.Flags]
        public enum EWaveModifier
        {
            None = 0,
            Gigantism = 1 << 0, // 1.35x Size
            Dwarfing = 1 << 1,  // 0.6x Size
            SuperSpeed = 1 << 2, // 2.0x Speed
            Zombified = 1 << 3, // Zombie logic
            Athletic = 1 << 4   // High stamina/speed
        }
        private EWaveModifier _currentModifier = EWaveModifier.None;

        public void StartNewWave()
        {
            if (IsWaveActive || !SurvivalEnabled) return;
            _isLost = false;

            // If we are at Wave 0 and haven't done prep yet, start prep.
            if (_waveManager.CurrentWave == 0 && !_isInitialPrep && !_prepPhaseDone) 
            {
                // Clear any leftover effects from previous runs
                DrugDebugModule.Instance.ClearActiveEffect();

                // Prepare world before 60s timer
                CaptureInventory();
                ClearWorldOfNPCs();
                ClearVans();
                StartArmingPhase(); // Spawn vans now so they have 60s to use them
                
                _isInitialPrep = true;
                _initialPrepTimeRemaining = 60f;
                MelonLogger.Msg("[Survival] Initial Preparation Phase Started (60s). Press Enter to skip.");
                return;
            }

            _prepPhaseDone = false; // Reset for next death cycle

            // 2. Wave Start (Enemies + Effects + Modifiers)
            if (_waveManager.CurrentWave > 0)
            {
                ClearVans();
                StartArmingPhase();
            }
            
            _waveManager.StartNextWave();
            IsWaveActive = true;
            MelonLogger.Msg($"[Survival] !!! V8.5 PATCH LOADED !!! Wave {ToRoman(_waveManager.CurrentWave)} Started. Enemies: {_waveManager.TotalEnemiesForWave}");

            // Boss Logic: Trigger Boss Handler
            HandleBossSpawning(_waveManager.CurrentWave);
            
            // Generate stacking modifiers (70% chance per wave)
            _currentModifier = EWaveModifier.None;
            
            if (Random.value < 0.70f)
            {
                // Roll for Size (Gigantism removed from enemies)
                float sizeRoll = Random.value;
                if (sizeRoll < 0.5f) _currentModifier |= EWaveModifier.Dwarfing;

                // Roll for Speed/Athletic
                float speedRoll = Random.value;
                if (speedRoll < 0.3f) _currentModifier |= EWaveModifier.SuperSpeed;
                else if (speedRoll < 0.6f) _currentModifier |= EWaveModifier.Athletic;

                // Roll for Zombified
                if (Random.value < 0.25f) _currentModifier |= EWaveModifier.Zombified;
            }

            MelonLogger.Msg($"[Survival] Active Wave Modifiers: {_currentModifier}");

            ApplyWaveEffects();
        }

        private bool _prepPhaseDone = false;

        private void ApplyWaveEffects()
        {
            var allEffects = DrugDebugModule.Instance.GetAllEffects();
            if (allEffects.Count == 0) return;

            // Filter out unwanted effects
            List<Effect> pool = allEffects.FindAll(e => 
                !e.ID.ToLower().Contains("explosive") && 
                !e.ID.ToLower().Contains("lethal")
            );
            
            foreach (var p in Player.PlayerList)
            {
                if (p == null || !p.Health.IsAlive) continue;
                
                // Refill pool if we have more players than unique effects
                if (pool.Count == 0) pool.AddRange(allEffects.FindAll(e => !e.ID.ToLower().Contains("explosive") && !e.ID.ToLower().Contains("lethal")));
                
                int idx = Random.Range(0, pool.Count);
                Effect e = pool[idx];
                pool.RemoveAt(idx);

                if (p == Player.Local)
                {
                    DrugDebugModule.Instance.ApplyEffect(e);
                }
                else
                {
                    // Apply mechanically to remote players
                    try { e.ApplyToPlayer(p); } catch {}
                }
            }
        }

        private void StartInitialPrep()
        {
            _isInitialPrep = true;
            _initialPrepTimeRemaining = 60f;
            MelonLogger.Msg("[Survival] Initial Preparation Phase Started (60s). Mass-Expanding Goon Allotment (50 units)...");
            
            // V8.5: Increase pre-allocated allotment immediately to avoid mid-wave cloning bugs
            ExpandGoonPool(50);

            MelonLogger.Msg("[Survival] Preparation Phase Ready. Press Enter to skip.");
        }

        public void AddActiveBoss(NPC npc) { if(npc != null && !_activeBosses.Contains(npc)) _activeBosses.Add(npc); }
        public List<NPC> GetActiveBosses() { _activeBosses.RemoveAll(b => b == null || b.Health.IsDead || !b.gameObject.activeInHierarchy); return _activeBosses; }
        public void AddActiveCivilian(NPC npc) { if(npc != null && !_activeCivilians.Contains(npc)) _activeCivilians.Add(npc); }
        public void IncrementSpawnCount() { _waveManager.OnEnemySpawned(); }

        private void HandleBossSpawning(int wave)
        {
            if (wave % 5 != 0) return;

            // Health scales: 1000, 2000, 3000, 4000...
            float bossHealth = (wave / 5) * 1000f;
            MelonLogger.Msg($"[Survival] WAVE {wave} BOSS ENCOUNTER: Summoning Champion ({bossHealth} HP)!");

            Vector3 pos = GetValidSpawnPosition();
            if (pos == Vector3.zero) return;

            var bossTemplate = SurvivalBossManager.Instance.GetRandomBoss();
            var target = GetNearestPlayer(pos);
            SurvivalBossManager.Instance.SpawnBoss(bossTemplate, pos, target, bossHealth);
        }

        public float GetProceduralChance()
        {
            if (_waveManager == null) return 0f;
            return WindyFramework.Modules.Enemies.EnemyScalingManager.GetMutationChance(_waveManager.CurrentWave);
        }

        private void SpawnEnemyLogic()
        {
            // HARD-LOCK: Standard NPCs never spawn during Boss Waves
            if (_waveManager.IsBossWave) return;

            // Calculate how many we CAN spawn based on limits
            int gRoom = MAX_ACTIVE_GOONS - _activeGoons.Count;
            int cRoom = MAX_ACTIVE_COPS - _activeCops.Count;
            int civRoom = 10 - _activeCivilians.Count; // Keep civs low
            int totalRoom = gRoom + cRoom + civRoom;

            if (totalRoom <= 0) return;

            int remainingForWave = _waveManager.TotalEnemiesForWave - _waveManager.SpawnedEnemies;
            
            // V7: Horde Logic - Spawn massive waves (up to 30) instantly
            int burstSize;
            if (remainingForWave < 30 && remainingForWave <= totalRoom)
            {
                burstSize = remainingForWave;
            }
            else
            {
                // Standard pacing for extremely large waves (still aggressive)
                burstSize = 10;
                burstSize = Mathf.Min(burstSize, remainingForWave);
                burstSize = Mathf.Min(burstSize, totalRoom);
            }
            
            // V8: Get EXACT distinct spawn points for this burst
            List<Vector3> spawnPoints = GetValidSpawnPoints(burstSize);
            
            if (spawnPoints.Count == 0) return; // Try again next tick

            MelonLogger.Msg($"[Survival] Spawning {spawnPoints.Count} enemies at exact coordinates.");

            int spawnedInBurst = 0;
            
            foreach (Vector3 p in spawnPoints)
            {
                if (_waveManager.SpawnedEnemies >= _waveManager.TotalEnemiesForWave) break;

                // Refresh room counts
                gRoom = MAX_ACTIVE_GOONS - _activeGoons.Count;
                cRoom = MAX_ACTIVE_COPS - _activeCops.Count;
                civRoom = 10 - _activeCivilians.Count;

                float rand = Random.value;
                bool success = false;
                
                // V8: Spawn EXACTLY at the validated point + 0.5m Up to prevent floor clipping
                // No random offset.
                Vector3 spawnPos = p + Vector3.up * 0.5f;

                // Procedural Mutation Check
                if (Random.value < GetProceduralChance())
                {
                    success = SpawnProceduralEnemy(spawnPos);
                }
                else
                {
                    if (rand < 0.33f && cRoom > 0) success = SpawnOneCop(spawnPos);
                    else if (rand < 0.66f && gRoom > 0) success = SpawnOneGoon(spawnPos);
                    else if (civRoom > 0) success = SpawnOneCivilian(spawnPos);
                }

                // Fallback if random choice failed
                if (!success)
                {
                    if (gRoom > 0) success = SpawnOneGoon(spawnPos);
                    else if (cRoom > 0) success = SpawnOneCop(spawnPos);
                    else if (civRoom > 0) success = SpawnOneCivilian(spawnPos);
                }

                if (success) spawnedInBurst++;
            }

            MelonLogger.Msg($"[Survival] Burst Result: {spawnedInBurst}/{burstSize} spawned.");

            if (spawnedInBurst > 0)
            {
                UpdateGroundTruthCache();
                _spawnTimer = 1.0f; // Brief delay between bursts
            }
            else
            {
                _spawnTimer = 0.5f; // Faster retry if burst failed
            }
        }

        private void AttachMonitor(NPC npc)
        {
            if (npc == null) return;
            var monitor = npc.gameObject.GetComponent<EnemyMonitor>();
            if (monitor == null) monitor = npc.gameObject.AddComponent<EnemyMonitor>();
            monitor.Setup(npc);
        }

        private bool SpawnProceduralEnemy(Vector3 pos)
        {
            if (Player.Local == null) return false;

            // 1. Pick a base template from the registry (any civilian goon or cop)
            var templates = NPCManager.NPCRegistry.FindAll(n => n != null && !IsNPCBlocked(n));
            if (templates.Count == 0) return false;
            
            NPC templateNPC = templates[Random.Range(0, templates.Count)];
            GameObject templateGO = templateNPC.gameObject;

            // 2. Clone via Assembler
            GameObject enemyGO = WindyFramework.Modules.Enemies.ProceduralAssembler.Instance.CloneTemplate(templateGO);
            if (enemyGO == null) return false;

            NPC npc = enemyGO.GetComponent<NPC>();
            if (npc == null) 
            {
                Destroy(enemyGO);
                return false;
            }

            // 3. Generate Stats based on Wave via Scaling Manager
            WindyFramework.Modules.Enemies.EnemyStats stats = WindyFramework.Modules.Enemies.EnemyScalingManager.GenerateStatsForWave(_waveManager.CurrentWave);

            // 4. Apply Stats
            WindyFramework.Modules.Enemies.ProceduralAssembler.Instance.ApplyStats(enemyGO, stats);

            // 5. Position and Initialize
            // V5.4: Ultra-Safe Teleport Pattern to ensure NavMesh registration
            if (npc.Movement != null && npc.Movement.Agent != null)
            {
                npc.Movement.Agent.enabled = false;
                npc.transform.position = pos;
                npc.Movement.Agent.enabled = true;
                npc.Movement.Warp(pos); // Sync network and state
            }
            else
            {
                npc.transform.position = pos;
            }

            npc.gameObject.SetActive(true);
            
            // Add Debug Label
            var dl = npc.gameObject.AddComponent<DebugLabel>();
            dl.Setup(npc, $"MUTANT: {npc.fullName}", new Color(1f, 0.5f, 0f)); // Orange for mutants

            // Attach Monitor
            AttachMonitor(npc);

            // Force Combat
            ForceCombatState(npc, GetNearestPlayer(pos));

            _waveManager.OnEnemySpawned();
            
            // We'll track it in the active list that matches its base type or just civilian for now
            if (npc is CartelGoon g) _activeGoons.Add(g);
            else if (npc is PoliceOfficer c) _activeCops.Add(c);
            else _activeCivilians.Add(npc);

            MelonLogger.Msg($"[Survival] Spawned Procedural MUTANT ({npc.fullName}) at {pos}");
            return true;
        }

        private void CleanupOrphanedLabels()
        {
            var labels = GameObject.FindObjectsOfType<DebugLabel>();
            foreach (var label in labels)
            {
                if (label == null) continue;
                
                var npc = label.GetComponent<NPC>();
                // If label has no NPC, or NPC is dead, or NPC is inactive - kill the label
                if (npc == null || npc.Health == null || npc.Health.IsDead || !npc.gameObject.activeInHierarchy)
                {
                    GameObject.Destroy(label);
                }
            }
        }

        public void UpdateGroundTruthCache()
        {
            int count = 0;
            var labels = GameObject.FindObjectsOfType<DebugLabel>();
            foreach(var dl in labels)
            {
                var n = dl.GetComponent<NPC>();
                if (n != null && n.Health != null && !n.Health.IsDead && n.gameObject.activeInHierarchy)
                {
                    count++;
                }
            }
            _cachedAliveCount = count;
        }

        private bool IsNPCBlocked(NPC npc)
        {
            if (npc == null) return true;
            
            // 1. Type-based blocking (Bosses)
            if (npc is ScheduleOne.NPCs.CharacterClasses.SewerGoblin || 
                npc is ScheduleOne.NPCs.CharacterClasses.Karen) return true;

            // 2. Name-based blocking
            if (string.IsNullOrEmpty(npc.fullName)) return false;
            string n = npc.fullName.ToLower();
            return n.Contains("nelson") || n.Contains("thomas benzies") || n.Contains("igor romanovich") || n.Contains("stranger") || n.Contains("stan carney") || n.Contains("manny oakfield") || n.Contains("walter cussler");
        }

        private bool SpawnOneCivilian(Vector3 overridePos = default)
        {
            if (_waveManager.IsBossWave) return false;

            var civilians = NPCManager.NPCRegistry.FindAll(n => 
                n != null && 
                !n.gameObject.activeSelf && 
                !(n is CartelGoon) && 
                !(n is PoliceOfficer) &&
                !(n is ScheduleOne.Employees.Employee) &&
                !(n is ScheduleOne.Economy.Dealer) &&
                !(n is ScheduleOne.Economy.Supplier) &&
                !IsNPCBlocked(n)
            );
            if (civilians.Count == 0 || Player.Local == null) return false;

            Vector3 pos = (overridePos != default) ? overridePos : GetValidSpawnPosition();
            if (pos == Vector3.zero) return false;

            var npc = civilians[Random.Range(0, civilians.Count)];
            
            // V5.4: Ultra-Safe Teleport Pattern
            if (npc.Movement != null && npc.Movement.Agent != null)
            {
                npc.Movement.Agent.enabled = false;
                npc.transform.position = pos;
                npc.Movement.Agent.enabled = true;
                npc.Movement.Warp(pos);
            }
            else
            {
                npc.transform.position = pos;
            }

            // FORCE ACTIVE & VISIBLE
            npc.gameObject.SetActive(true);
            if (npc.Avatar != null)
            {
                if (npc.Avatar.BodyContainer != null) npc.Avatar.BodyContainer.gameObject.SetActive(true);
                npc.Avatar.SetRagdollPhysicsEnabled(false);
            }

            // Aggressive Reset
            if (npc.Health.IsDead) npc.Health.Revive();
            npc.Health.RestoreHealth();

            if (npc.Avatar != null && npc.Avatar.Animation != null && npc.Avatar.Animation.animator != null)
            {
                var anim = npc.Avatar.Animation.animator;
                anim.SetBool("Sitting", false);
                anim.Play(0, 0, 0f); // Reset to base layer start
            }

            // Add Debug Label
            var oldDl = npc.gameObject.GetComponent<DebugLabel>();
            if (oldDl != null) Destroy(oldDl);
            var dl = npc.gameObject.AddComponent<DebugLabel>();
            dl.Setup(npc, $"CIV: {npc.fullName}", Color.white);

            ApplyModifier(npc);
            if (npc.Inventory != null) npc.Inventory.SlotCount = 10;

            AttachMonitor(npc);

            MelonLogger.Msg($"[Survival] Spawning Civilian ({npc.GetType().Name}) at {pos}");
            
            _activeCivilians.Add(npc);
            _waveManager.OnEnemySpawned();

            // Unified Aggressive Engagement
            var target = GetNearestPlayer(pos);
            ForceCombatState(npc, target);

            // Weapon assignment for later waves (50% chance, starts later)
            if (_waveManager.CurrentWave >= 5 && Random.value < 0.5f)
            {
                try
                {
                    string weaponPath = GetWeaponForWave(_waveManager.CurrentWave);
                    npc.Avatar.SetEquippable(weaponPath);
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"[Survival] Failed to set weapon for {npc.fullName}: {ex.Message}");
                }
            }

            return true;
        }

        public void ForceCombatState(NPC npc, Player target)
        {
            if (npc == null || target == null) return;

            // 1. Boost AI Stats for maximum aggression
            npc.Aggression = 1.0f;

            // 2. DISABLE PASSIVE BEHAVIORS (Keep them hunting)
            if (npc.Behaviour != null)
            {
                if (npc.Behaviour.FleeBehaviour != null) npc.Behaviour.FleeBehaviour.enabled = false;
                if (npc.Behaviour.CoweringBehaviour != null) npc.Behaviour.CoweringBehaviour.enabled = false;
            }

            // 3. Force Aggro via CombatBehaviour (Base engagement)
            if (npc.Behaviour?.CombatBehaviour != null)
            {
                try 
                { 
                    npc.Behaviour.CombatBehaviour.enabled = true;
                    npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(target.NetworkObject); 
                } catch {}
            }

            // 4. Class-specific pursuit triggers
            if (npc is CartelGoon goon)
            {
                goon.AttackEntity(target);
            }
            else if (npc is PoliceOfficer cop)
            {
                cop.BeginFootPursuit_Networked(target.PlayerCode);
            }
            else
            {
                // Generic NPC (like Karen) fallback - Force movement to player if stuck
                if (npc.Movement != null)
                {
                    npc.Movement.MovementSpeedScale = 1.0f; // Force Run speed logic
                    npc.Movement.ResumeMovement();
                    
                    if (Vector3.Distance(npc.transform.position, target.transform.position) > 2f)
                    {
                        npc.Movement.SetDestination(target.transform.position);
                    }
                }
            }
        }

        private bool SpawnOneCop(Vector3 overridePos = default)
        {
            if (Player.Local == null) return false;
            var station = PoliceStation.GetClosestPoliceStation(Player.Local.transform.position);
            if(!station || station.OfficerPool.Count == 0) return false;
            
            Vector3 pos = (overridePos != default) ? overridePos : GetValidSpawnPosition();
            if (pos == Vector3.zero) return false;

            PoliceOfficer cop = station.PullOfficer(); if (!cop) return false;
            
            // V5.4: Ultra-Safe Teleport Pattern
            if (cop.Movement != null && cop.Movement.Agent != null)
            {
                cop.Movement.Agent.enabled = false;
                cop.transform.position = pos;
                cop.Movement.Agent.enabled = true;
                cop.Movement.Warp(pos);
            }
            else
            {
                cop.transform.position = pos;
            }

            MelonLogger.Msg($"[Survival] Spawning Cop at {pos}");

            // Add Debug Label
            var oldDl = cop.gameObject.GetComponent<DebugLabel>();
            if (oldDl != null) Destroy(oldDl);
            var dl = cop.gameObject.AddComponent<DebugLabel>();
            dl.Setup(cop, $"COP: {cop.fullName}", Color.blue);

            ApplyModifier(cop);

            if (cop.Inventory != null) cop.Inventory.SlotCount = 10;

            if (_waveManager.CurrentWave < 8 && cop.BatonPrefab != null) cop.GunPrefab = cop.BatonPrefab;
            
            AttachMonitor(cop);

            // Unified Aggressive Engagement
            ForceCombatState(cop, GetNearestPlayer(pos));
            
            _activeCops.Add(cop); _waveManager.OnEnemySpawned(); return true;
        }

        private void ExpandGoonPool(int count)
        {
            if (!InstanceFinder.IsServer) return;
            var pool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance?.GoonPool;
            if (pool == null) return;

            // Get unspawned list via reflection
            var field = typeof(GoonPool).GetField("unspawnedGoons", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = (List<CartelGoon>)field.GetValue(pool);
            
            // Get a template (either from list or find one in scene)
            CartelGoon template = null;
            if (list.Count > 0) template = list[0];
            else 
            {
                var all = FindObjectsOfType<CartelGoon>();
                if (all.Length > 0) template = all[0];
            }

            if (template == null) 
            {
                MelonLogger.Error("[Survival] Cannot expand GoonPool: No template found.");
                return;
            }

            MelonLogger.Msg($"[Survival] Expanding GoonPool by {count} clones of {template.name}...");

            // V7.1: Deactivate template to prevent Awake() from running on clone immediately
            // This allows us to clean up runtime components before they conflict
            bool wasActive = template.gameObject.activeSelf;
            template.gameObject.SetActive(false);

            try
            {
                for(int i=0; i<count; i++)
                {
                    GameObject go = Instantiate(template.gameObject);
                    go.name = $"{template.name}_Expanded_{System.Guid.NewGuid().ToString().Substring(0,4)}";
                    
                    // V8.6: Minimal Sanitization - Let Spawn() handle the rest
                    CartelGoon newGoon = go.GetComponent<CartelGoon>();
                    if (newGoon != null)
                    {
                        if (newGoon.Health != null)
                        {
                            var healthField = typeof(NPCHealth).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);
                            healthField?.SetValue(newGoon.Health, 100f);
                            var isDeadField = typeof(NPCHealth).GetField("isDead", BindingFlags.NonPublic | BindingFlags.Instance);
                            isDeadField?.SetValue(newGoon.Health, false);
                        }
                    }

                    // Move far away to avoid Start() physics pops
                    go.transform.position = new Vector3(0, -500, 0);

                    go.SetActive(true); 
                    InstanceFinder.ServerManager.Spawn(go);
                    
                    // Force state to "Unspawned" so the official Spawn() works
                    var isGoonSpawnedProp = typeof(CartelGoon).GetProperty("IsGoonSpawned", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    isGoonSpawnedProp?.SetValue(newGoon, false, null);

                    newGoon.gameObject.SetActive(false);
                    list.Add(newGoon);
                }
            }
            finally
            {
                if(wasActive) template.gameObject.SetActive(true);
            }
            
            MelonLogger.Msg($"[Survival] GoonPool Expanded. New Count: {list.Count}");
        }

        private bool SpawnOneGoon(Vector3 overridePos = default)
        {
            if (NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance == null)
            {
                MelonLogger.Warning("[Survival] SpawnOneGoon Failed: Cartel Instance is Null");
                return false;
            }
            var pool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.GoonPool;
            if (pool == null || Player.Local == null) 
            {
                MelonLogger.Warning($"[Survival] SpawnOneGoon Failed: Pool={pool!=null}, Player={Player.Local!=null}");
                return false;
            }

            // V6: Auto-Expand Pool if running low
            var field = typeof(GoonPool).GetField("unspawnedGoons", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = (List<CartelGoon>)field.GetValue(pool);
            if (list.Count < 3)
            {
                ExpandGoonPool(5); // Always keep a buffer
            }

            Vector3 pos = (overridePos != default) ? overridePos : GetValidSpawnPosition(); 
            if (pos == Vector3.zero) return false;
            
            CartelGoon goon = null; 
            for(int i=0; i<3; i++) { goon = pool.SpawnGoon(pos); if(goon != null) break; }
            
            if (goon) {
                // V5.4: Ultra-Safe Teleport Pattern
                if (goon.Movement != null && goon.Movement.Agent != null)
                {
                    goon.Movement.Agent.enabled = false;
                    goon.transform.position = pos;
                    goon.Movement.Agent.enabled = true;
                    goon.Movement.Warp(pos);
                }
                else
                {
                    goon.transform.position = pos;
                }

                // 1. TEMPORARILY BYPASS THE AUTO-HIDE LISTENER
                // CartelGoon.Start adds Health.onRevive.AddListener(Despawn)
                // We must remove it so Revive() doesn't immediately hide/despawn the goon
                goon.Health.onRevive.RemoveListener(goon.Despawn);

                // 2. FORCE ACTIVE & VISIBLE
                goon.gameObject.SetActive(true);
                goon.SetVisible(true);
                
                var prop = typeof(CartelGoon).GetProperty("IsGoonSpawned", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                prop?.SetValue(goon, true, null);

                if (goon.Avatar != null)
                {
                    goon.Avatar.SetVisible(true);
                    if (goon.Avatar.BodyContainer != null) goon.Avatar.BodyContainer.gameObject.SetActive(true);
                    goon.Avatar.SetRagdollPhysicsEnabled(false);
                    
                    // Hyper-Aggressive mesh force
                    if (goon.Avatar.FaceMesh != null) goon.Avatar.FaceMesh.enabled = true;
                    if (goon.Avatar.BodyMeshes != null)
                    {
                        foreach(var m in goon.Avatar.BodyMeshes) if(m != null) m.enabled = true;
                    }
                    foreach(var smr in goon.GetComponentsInChildren<SkinnedMeshRenderer>(true)) smr.enabled = true;
                }

                // 3. Aggressive Reset
                if (goon.Health.IsDead) goon.Health.Revive();
                goon.Health.RestoreHealth();

                if (goon.Avatar != null && goon.Avatar.Animation != null && goon.Avatar.Animation.animator != null)
                {
                    var anim = goon.Avatar.Animation.animator;
                    anim.SetBool("Sitting", false);
                    anim.Play(0, 0, 0f);
                }

                // 4. RE-ADD THE LISTENER (So normal cleanup works after survival wave logic)
                goon.Health.onRevive.RemoveListener(goon.Despawn); // Ensure no dupes
                goon.Health.onRevive.AddListener(goon.Despawn);

                // Add Debug Label
                var oldDl = goon.gameObject.GetComponent<DebugLabel>();
                if (oldDl != null) Destroy(oldDl);
                var dl = goon.gameObject.AddComponent<DebugLabel>();
                dl.Setup(goon, $"GOON: {goon.fullName}", Color.red);

                ApplyModifier(goon);
                if (goon.Inventory != null) goon.Inventory.SlotCount = 10;

                AttachMonitor(goon);

                MelonLogger.Msg($"[Survival] Spawning Goon at {pos}");
                
                // Unified Aggressive Engagement
                ForceCombatState(goon, GetNearestPlayer(pos));

                if (_waveManager.CurrentWave >= 4)
                {
                    // 70% chance for weapons, otherwise fists
                    if (Random.value < 0.7f)
                    {
                        string weaponPath = GetWeaponForWave(_waveManager.CurrentWave);
                        goon.Avatar.SetEquippable(weaponPath);
                    }
                }
                
                _activeGoons.Add(goon); _waveManager.OnEnemySpawned(); return true;
            }
            else
            {
                MelonLogger.Warning("[Survival] SpawnOneGoon Failed: Pool returned null (Empty?)");
            }
            return false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                SaveCurrentLocationAsSpawnPoint();
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                DebugLabel.ShowBeams = !DebugLabel.ShowBeams;
                MelonLogger.Msg($"[Survival] Enemy Beams: {(DebugLabel.ShowBeams ? "ENABLED" : "DISABLED")}");
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                DebugLabel.ShowLabels = !DebugLabel.ShowLabels;
                MelonLogger.Msg($"[Survival] Enemy Names: {(DebugLabel.ShowLabels ? "ENABLED" : "DISABLED")}");
            }

            // 1. Hud Alpha Control
            float targetAlpha = 0f;
            if (SurvivalEnabled && !_isLost)
            {
                targetAlpha = (IsWaveActive || _isInitialPrep || _isWaitingForNextWave) ? 1.0f : 0.4f;
            }
            _hudAlpha = Mathf.MoveTowards(_hudAlpha, targetAlpha, Time.deltaTime * 1.5f);

            // Boss HUD Alpha Control (Elden Ring style fade)
            float targetBossAlpha = (SurvivalEnabled && GetActiveBosses().Count > 0) ? 1.0f : 0f;
            _bossHudAlpha = Mathf.MoveTowards(_bossHudAlpha, targetBossAlpha, Time.deltaTime * 1.0f);

            // 2. Wave Completion Check
            if (IsWaveActive && !_isWaitingForNextWave && !_isAwaitingTransition)
            {
                if (_waveManager.IsWaveComplete && _cachedAliveCount <= 0)
                {
                    MelonLogger.Msg($"[Survival] Wave {_waveManager.CurrentWave} Complete! (Quota Met & Ground Truth Verified)");
                    IsWaveActive = false;
                    _isAwaitingTransition = true;
                    StartCoroutine(AutoStartNextWave());
                }
            }

            if (!SurvivalEnabled || _isLost || Player.Local == null) return;

            // Aggro Enforcement (Every 5 seconds)
            if (IsWaveActive && Time.time % 5f < Time.deltaTime)
            {
                foreach(var g in _activeGoons) if(g && !g.Health.IsDead) ForceCombatState(g, GetNearestPlayer(g.transform.position));
                foreach(var c in _activeCops) if(c && !c.Health.IsDead) ForceCombatState(c, GetNearestPlayer(c.transform.position));
                foreach(var civ in _activeCivilians) if(civ && !civ.Health.IsDead) ForceCombatState(civ, GetNearestPlayer(civ.transform.position));
            }

            // High-Aggression Safety Sweep (Every 10 seconds)
            // Kills anyone who has drifted too far or is bugged/invisible
            // We ignore NPCs that were JUST spawned (less than 10s old)
            if (IsWaveActive && Time.time % 10f < Time.deltaTime)
            {
                var allEnemies = GameObject.FindObjectsOfType<NPC>();
                foreach(var n in allEnemies)
                {
                    if (n == null || n.Health.IsDead || !n.gameObject.activeInHierarchy) continue;
                    
                    // Only target survival-labeled enemies
                    var label = n.GetComponent<DebugLabel>();
                    if (label != null)
                    {
                        // Check lifetime - if they just spawned, give them a chance
                        var spawnTimeField = label.GetType().GetField("_spawnTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        float spawnTime = (float)(spawnTimeField?.GetValue(label) ?? 0f);
                        if (Time.time - spawnTime < 15f) continue;

                        var nearest = GetNearestPlayer(n.transform.position);
                        if (nearest != null && Vector3.Distance(n.transform.position, nearest.transform.position) > 150f)
                        {
                            MelonLogger.Warning($"[Survival] Safety Sweep killing lost enemy: {n.fullName}");
                            n.Health.TakeDamage(9999f);
                        }
                    }
                }
            }

            // List Cleanup/Validation (Every 3 seconds)
            // This catches NPCs that died but didn't register for some reason
            if (IsWaveActive && Time.time % 3f < Time.deltaTime)
            {
                _activeGoons.RemoveAll(g => !g || g.Health.IsDead);
                _activeCops.RemoveAll(c => !c || c.Health.IsDead);
                _activeCivilians.RemoveAll(civ => !civ || civ.Health.IsDead);

                // Aggressive Label Cleanup (Prevent ghost markers)
                CleanupOrphanedLabels();

                // Update 'Ground Truth' cache here (Heavy operation moved out of OnGUI)
                UpdateGroundTruthCache();
            }

            if (_isInitialPrep)
            {
                _initialPrepTimeRemaining -= Time.deltaTime;
                if (_initialPrepTimeRemaining <= 0 || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    _isInitialPrep = false;
                    _prepPhaseDone = true;
                    _initialPrepTimeRemaining = 0;
                    StartNewWave();
                }
                return;
            }

            if (IsWaveActive) {
                _spawnTimer += Time.deltaTime;
                float delay = _waveManager.GetSpawnDelay();
                if (_spawnTimer > delay && _waveManager.CanSpawnMore()) { _spawnTimer = 0f; SpawnEnemyLogic(); }
            }
            CleanupDead();
            
            if (IsWaveActive && !_waveManager.CanSpawnMore() && _activeGoons.Count == 0 && _activeCops.Count == 0 && _activeCivilians.Count == 0) 
            {
                MelonLogger.Msg($"[Survival] Wave {ToRoman(_waveManager.CurrentWave)} Cleared!");
                IsWaveActive = false;
            }

            if (SurvivalEnabled && !IsWaveActive && _activeGoons.Count == 0 && _activeCops.Count == 0 && _activeCivilians.Count == 0 && !_isWaitingForNextWave && !_isInitialPrep) 
            {
                StartCoroutine(AutoStartNextWave());
            }
        }

        private void CleanupDead()
        {
            Vector3 pPos = Player.Local != null ? Player.Local.transform.position : Vector3.zero;

            // Cleanup Goons
            for (int i = _activeGoons.Count - 1; i >= 0; i--) 
            {
                var g = _activeGoons[i];
                bool isDead = g && g.Health.IsDead;
                bool shouldRemove = !g || isDead;
                
                if (!shouldRemove)
                {
                    float d = Vector3.Distance(g.transform.position, GetNearestPlayer(g.transform.position).transform.position);
                    if (d > 300f || g.transform.position.y < -50f)
                    {
                        MelonLogger.Warning($"[Survival] Force-Cleaning Goon (Lost at dist {d})");
                        g.Health.TakeDamage(9999f); // This makes them dead, but we treat it as refund for quota?
                        // Actually, if we kill them, they are dead. But "lost" shouldn't count as a player kill?
                        // Spec says: "Deaths caused by "bad spawns" ... must not count towards the wave progress."
                        // So we should NOT count this as a kill in WaveManager.
                        // However, TakeDamage makes IsDead true.
                        // We need to handle the accounting here.
                        
                        // Let's just remove them without TakeDamage if possible, or ignore the IsDead check next frame?
                        // If we remove from list, we need to decide right now.
                        
                        shouldRemove = true;
                        _waveManager.OnSpawnRefunded(); // Refund quota
                    }
                }
                else if (isDead)
                {
                    _waveManager.OnEnemyKilled();
                }
                else if (!g) // Null/Destroyed externally
                {
                    _waveManager.OnSpawnRefunded();
                }

                if (shouldRemove) { if(g) StartCoroutine(RecycleGoonRoutine(g)); _activeGoons.RemoveAt(i); }
            }

            // Cleanup Cops
            for (int i = _activeCops.Count - 1; i >= 0; i--)
            {
                var c = _activeCops[i];
                bool isDead = c && c.Health.IsDead;
                bool shouldRemove = !c || isDead;

                if (!shouldRemove)
                {
                    float d = Vector3.Distance(c.transform.position, GetNearestPlayer(c.transform.position).transform.position);
                    if (d > 300f || c.transform.position.y < -50f)
                    {
                        MelonLogger.Warning($"[Survival] Force-Cleaning Cop (Lost at dist {d})");
                        c.Health.TakeDamage(9999f);
                        shouldRemove = true;
                        _waveManager.OnSpawnRefunded();
                    }
                }
                else if (isDead)
                {
                    _waveManager.OnEnemyKilled();
                }
                else if (!c)
                {
                    _waveManager.OnSpawnRefunded();
                }

                if (shouldRemove) { if(c) StartCoroutine(RecycleCopRoutine(c)); _activeCops.RemoveAt(i); }
            }

            // Cleanup Civilians
            for (int i = _activeCivilians.Count - 1; i >= 0; i--)
            {
                var n = _activeCivilians[i];
                bool isDead = n && n.Health.IsDead;
                bool shouldRemove = !n || isDead;

                if (!shouldRemove)
                {
                    float d = Vector3.Distance(n.transform.position, GetNearestPlayer(n.transform.position).transform.position);
                    if (d > 300f || n.transform.position.y < -50f)
                    {
                        MelonLogger.Warning($"[Survival] Force-Cleaning Civilian (Lost at dist {d})");
                        n.Health.TakeDamage(9999f);
                        shouldRemove = true;
                        _waveManager.OnSpawnRefunded();
                    }
                }
                else if (isDead)
                {
                    _waveManager.OnEnemyKilled();
                }
                else if (!n)
                {
                    _waveManager.OnSpawnRefunded();
                }

                if (shouldRemove) { if(n) { DrugDebugModule.Instance.ClearEffectsFromNPC(n); n.gameObject.SetActive(false); } _activeCivilians.RemoveAt(i); }
            }
        }

        private IEnumerator RecycleGoonRoutine(CartelGoon g) 
        { 
            yield return new WaitForSeconds(5f); 
            if(g != null) 
            { 
                DrugDebugModule.Instance.ClearEffectsFromNPC(g); 
                // Ensure we don't return to pool if it was already destroyed or returned
                if (g.gameObject != null)
                {
                    try {
                        NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.GoonPool.ReturnToPool(g); 
                        g.Despawn(); 
                    } catch { }
                }
            } 
        }
        private IEnumerator RecycleCopRoutine(PoliceOfficer c) 
        { 
            yield return new WaitForSeconds(5f); 
            if(c != null) 
            { 
                DrugDebugModule.Instance.ClearEffectsFromNPC(c); 
                if (c.gameObject != null) c.Deactivate(); 
            } 
        }
        private void ClearVans() { foreach (var v in _activeVans) if (v) { if (v.NetworkObject?.IsSpawned == true) v.NetworkObject.Despawn(); else GameObject.Destroy(v.gameObject); } _activeVans.Clear(); }
        
        private void CaptureInventory() { var inv = PlayerSingleton<PlayerInventory>.Instance; var money = NetworkSingleton<MoneyManager>.Instance; if (!inv) return; _savedInventory.Clear(); for (int i = 0; i < inv.hotbarSlots.Count; i++) if (inv.hotbarSlots[i].ItemInstance != null) _savedInventory.Add(new SavedItem { Index = i, Instance = inv.hotbarSlots[i].ItemInstance }); _savedCash = inv.cashInstance != null ? inv.cashInstance.Balance : 0; _savedBankBalance = money != null ? money.onlineBalance : 0; inv.ClearInventory(); if (inv.cashInstance != null) inv.cashInstance.SetBalance(0); if (money != null && money.onlineBalance > 0) money.CreateOnlineTransaction("Survival Fee", -money.onlineBalance, 1, "Service"); }
        private void RestoreInventory() { var inv = PlayerSingleton<PlayerInventory>.Instance; var money = NetworkSingleton<MoneyManager>.Instance; if (!inv) return; inv.ClearInventory(); foreach (var s in _savedInventory) if (s.Index < inv.hotbarSlots.Count) inv.hotbarSlots[s.Index].SetStoredItem(s.Instance); if (inv.cashInstance != null) inv.cashInstance.SetBalance(_savedCash); if (money != null) { float d = _savedBankBalance - money.onlineBalance; if (d != 0) money.CreateOnlineTransaction("Survival Refund", d, 1, "Service"); } _savedInventory.Clear(); }
        
        private IEnumerator AutoStartNextWave(float delay = 10f) 
        { 
            _isWaitingForNextWave = true; 
            _nextWaveTimeRemaining = delay; 

            // Trigger Pool Refill and Scaling at the start of the transition
            RefillPools();
            ScalePools();

            while (_nextWaveTimeRemaining > 0) 
            { 
                _nextWaveTimeRemaining -= Time.deltaTime; 
                yield return null; 
            } 
            _isWaitingForNextWave = false; 
            _isAwaitingTransition = false;
            if (SurvivalEnabled) StartNewWave(); 
        }

        private void ScalePools()
        {
            int nextWave = _waveManager.CurrentWave + 1;
            int req = 5 + (nextWave - 1) * 2;
            
            // Total capacity currently available
            var goonPool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance?.GoonPool;
            int currentGoonCap = 0;
            if (goonPool != null)
            {
                var unspawnedField = typeof(ScheduleOne.Cartel.GoonPool).GetField("unspawnedGoons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var unspawnedList = unspawnedField?.GetValue(goonPool) as List<CartelGoon>;
                currentGoonCap = unspawnedList?.Count ?? 0;
            }

            int currentCopCap = 0;
            foreach (var station in PoliceStation.PoliceStations)
            {
                if (station != null) currentCopCap += station.OfficerPool.Count;
            }

            int totalCap = currentGoonCap + currentCopCap;
            
            if (totalCap < req)
            {
                int needed = req - totalCap;
                MelonLogger.Warning($"[Survival] Pool capacity ({totalCap}) is lower than next wave requirement ({req}). Borrowing {needed} civilians...");
                
                // Borrow ANY available civilian from registry
                var candidates = NPCManager.NPCRegistry.FindAll(n => 
                    n != null && 
                    !n.gameObject.activeInHierarchy && 
                    !(n is PoliceOfficer) && !(n is CartelGoon) &&
                    !IsNPCBlocked(n));

                int borrowed = 0;
                foreach (var civ in candidates)
                {
                    if (borrowed >= needed) break;
                    if (goonPool != null)
                    {
                        var unspawnedField = typeof(ScheduleOne.Cartel.GoonPool).GetField("unspawnedGoons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var unspawnedList = unspawnedField?.GetValue(goonPool) as List<CartelGoon>;
                        
                        // Fake a goon by setting up behavior and adding to pool
                        ResetNPC(civ);
                        if (unspawnedList != null && civ is CartelGoon cg)
                        {
                             unspawnedList.Add(cg);
                             borrowed++;
                        }
                        else
                        {
                            // If it's a base NPC, we track it in our own Civilian list instead
                            // SpawnEnemyLogic handles this by prioritized Civilian spawns
                            borrowed++; 
                        }
                    }
                }
                MelonLogger.Msg($"[Survival] Successfully boosted pool capacity by {borrowed} entities.");
            }
        }

        private void RefillPools()
        {
            MelonLogger.Msg("[Survival] Refilling NPC pools for next wave...");
            CleanupOrphanedLabels(); // Clear ghost markers before refilling
            ClearWorldOfNPCs();      // Physically remove lingering NPCs from world
            
            // 1. Reset and Return Goons
            var goonPool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance?.GoonPool;
            if (goonPool != null)
            {
                // Scan registry for ALL goons, not just scene-active ones
                foreach (var npc in NPCManager.NPCRegistry)
                {
                    if (npc is CartelGoon g)
                    {
                        if (g != null && (g.Health.IsDead || !g.gameObject.activeInHierarchy || !_activeGoons.Contains(g)))
                        {
                            ResetNPC(g);
                            
                            // Check spawning state via reflection to avoid log spam
                            bool isSpawned = false;
                            try {
                                var prop = typeof(CartelGoon).GetProperty("IsGoonSpawned", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                isSpawned = (bool)(prop?.GetValue(g) ?? false);
                            } catch {}

                            if (isSpawned)
                            {
                                try { goonPool.ReturnToPool(g); } catch {}
                                try { g.Despawn(); } catch {}
                            }
                        }
                    }
                }
            }

            // 2. Reset and Return Cops
            foreach (var station in PoliceStation.PoliceStations)
            {
                if (station == null) continue;
                
                // Scan registry for ALL cops
                foreach (var npc in NPCManager.NPCRegistry)
                {
                    if (npc is PoliceOfficer c)
                    {
                        if (c != null && (c.Health.IsDead || !c.gameObject.activeInHierarchy || !_activeCops.Contains(c)))
                        {
                            ResetNPC(c);
                            if (!station.OfficerPool.Contains(c)) station.OfficerPool.Add(c);
                            c.Deactivate();
                        }
                    }
                }
            }
        }

        private void ResetNPC(NPC npc)
        {
            if (npc == null) return;

            // 1. Restore Health
            npc.Health.MaxHealth = 100f;
            npc.Health.RestoreHealth();
            npc.Health.Revive();

            // 2. FORCIBLY Clear Drugs/Effects (Brute force via reflection)
            if (npc.Behaviour != null && npc.Behaviour.ConsumeProductBehaviour != null)
            {
                var behav = npc.Behaviour.ConsumeProductBehaviour;
                if (behav.ConsumedProduct != null)
                {
                    try { behav.ConsumedProduct.ClearEffectsFromNPC(npc); } catch {}
                    var prop = typeof(ScheduleOne.NPCs.Behaviour.ConsumeProductBehaviour).GetProperty("ConsumedProduct", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    prop?.SetValue(behav, null, null);
                }
            }

            // 3. Reset Scale and Speed
            npc.transform.localScale = Vector3.one;
            if (npc.Movement != null) npc.Movement.MoveSpeedMultiplier = 1.0f;

            // 4. Clear Inventory
            if (npc.Inventory != null) npc.Inventory.Clear();
        }

        public void StopWaves() { StopAllCoroutines(); IsWaveActive = false; foreach(var g in _activeGoons) if(g) g.Despawn(); foreach(var c in _activeCops) if(c) c.Deactivate(); foreach(var civ in _activeCivilians) if(civ) civ.gameObject.SetActive(false); _activeGoons.Clear(); _activeCops.Clear(); _activeCivilians.Clear(); ClearVans(); RestoreInventory(); _waveManager.Reset(); }
                        public void OnPlayerLost() 
                        { 
                            if (_isLost) return;
                            _isLost = true;
                            
                            // Lock Character & Hide Arms
                            if (Player.Local != null)
                            {
                                Player.Local.SetRagdolled(true);
                                PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
                                PlayerSingleton<PlayerCamera>.Instance.SetCanLook(false);
                                PlayerSingleton<PlayerInventory>.Instance.SetViewmodelVisible(false);
                                
                                // Clear all drug effects immediately
                                DrugDebugModule.Instance.ClearActiveEffect();
                
                                // Death Camera: Move back 3m and look down
                                Vector3 deathPos = Player.Local.transform.position + (Player.Local.transform.up * 4f) + (Player.Local.transform.forward * -3f);
                                Quaternion deathRot = Quaternion.LookRotation(Player.Local.transform.position - deathPos);
                                PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(deathPos, deathRot, 1.0f);
                
                                // Ensure mouse is freed for the death screen
                                PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
                            }
                
                            IsWaveActive = false;
                        }
        public void RespawnPlayer()
        {
            if (Player.Local == null) return;

            // 1. Reset Health & Physics
            Player.Local.Health.SetHealth(100f);
            Player.Local.SetRagdolled(false);
            
            // 2. Fix Camera
            PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
            PlayerSingleton<PlayerCamera>.Instance.SetCanLook(true);
            PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
            PlayerSingleton<PlayerInventory>.Instance.SetViewmodelVisible(true);
            PlayerSingleton<PlayerCamera>.Instance.LockMouse();
            
            // 3. Teleport & Clean World
            Vector3 customSpawn = SurvivalLaunch.GetRandomPlayerSpawn();
            if (customSpawn != Vector3.zero) PlayerSingleton<PlayerMovement>.Instance.Teleport(customSpawn);
            
            StopWaves(); // Clears NPCs and resets wave counter
            _isLost = false;
            
            // 4. Start Prep Phase again
            StartNewWave();
        }

        public void ForceSkipWave()
        {
            if (!SurvivalEnabled) return;

            MelonLogger.Msg("[Survival] Expediting to Next Wave...");

            // 1. Stop spawning more enemies
            _waveManager.OnWaveSkipped();

            // 2. Kill all active managed NPCs
            foreach (var g in _activeGoons) if (g && !g.Health.IsDead) g.Health.TakeDamage(9999f);
            foreach (var c in _activeCops) if (c && !c.Health.IsDead) c.Health.TakeDamage(9999f);
            foreach (var civ in _activeCivilians) if (civ && !civ.Health.IsDead) civ.Health.TakeDamage(9999f);

            // Clear lists so they don't ghost into the next wave's HUD count
            _activeGoons.Clear();
            _activeCops.Clear();
            _activeCivilians.Clear();

            // 3. Mark wave inactive and start an expedited countdown
            IsWaveActive = false;
            StopAllCoroutines(); // Stop any pending auto-starts
            StartCoroutine(AutoStartNextWave(1.0f)); // 1 second countdown instead of 10
        }

        private void ApplyModifier(NPC npc)
        {
            if (npc == null) return;

            // BOSS LOCK: If it's a boss wave, we don't apply standard modifiers
            // Champions only use their internal 1.2x scale logic
            if (_waveManager.IsBossWave) return;

            // CLEAR INVENTORY FIRST (To ensure clean state for loot and weapons)
            if (npc.Inventory != null) npc.Inventory.Clear();

            float scale = 1.0f;
            float speed = 1.0f;

            if ((_currentModifier & EWaveModifier.Gigantism) != 0)
            {
                scale = 1.35f; // Reduced from 2.0 for better hit reliability
                speed *= 0.9f; // Slightly faster than the 2.0x giants
            }
            else if ((_currentModifier & EWaveModifier.Dwarfing) != 0)
            {
                scale = 0.5f;
                speed *= 1.2f;
            }

            if ((_currentModifier & EWaveModifier.SuperSpeed) != 0) speed *= 2.2f; // Slight bump
            if ((_currentModifier & EWaveModifier.Athletic) != 0) speed *= 1.5f;

            npc.transform.localScale = Vector3.one * scale;
            if (npc.Movement != null) 
            {
                npc.Movement.MoveSpeedMultiplier = speed;
                // Force immediate agent update
                if (npc.Movement.Agent != null && npc.Movement.Agent.enabled)
                {
                    float baseSpeed = Mathf.Lerp(npc.Movement.WalkSpeed, npc.Movement.RunSpeed, npc.Movement.MovementSpeedScale);
                    npc.Movement.Agent.speed = baseSpeed * speed;
                }
            }

            if ((_currentModifier & EWaveModifier.Zombified) != 0)
            {
                var effects = DrugDebugModule.Instance.GetAllEffects();
                var zomb = effects.Find(e => e.ID.ToLower().Contains("zombifying"));
                if (zomb != null) try { zomb.ApplyToNPC(npc); } catch {}
            }

            // Anti-Clipping: Randomize stopping distance so they don't all fuse into the player
            // Reverted as per user request ("kept them from hitting me")
            if (npc.Movement != null && npc.Movement.Agent != null)
            {
                npc.Movement.Agent.stoppingDistance = 0.5f; // Standard close combat
                npc.Movement.Agent.autoBraking = true;
            }

            // Assign Random Loot Cash (10 - 20 dollars, single slot)
            if (npc.Inventory != null)
            {
                float loot = Random.Range(10f, 20f);
                var cashInstance = NetworkSingleton<ScheduleOne.Money.MoneyManager>.Instance.GetCashInstance(loot);
                npc.Inventory.InsertItem(cashInstance);
            }
        }

        // Public helper for patches to re-apply if scale/speed is reset
        public void ReApplyCurrentModifier(NPC npc)
        {
            ApplyModifier(npc);
        }
        private string ToRoman(int n) { if (n < 1) return ""; if (n >= 10) return "X" + ToRoman(n - 10); if (n >= 9) return "IX" + ToRoman(n - 9); if (n >= 5) return "V" + ToRoman(n - 5); if (n >= 4) return "IV" + ToRoman(n - 4); return "I" + ToRoman(n - 1); }
        private string GetWeaponForWave(int w) 
        { 
            if (w < 5) return "Avatar/Equippables/BaseballBat"; 
            if (w < 8) return "Avatar/Equippables/Machete"; 
            if (w < 12) return "Avatar/Equippables/Revolver"; 
            if (w < 15) return "Avatar/Equippables/M1911";
            return "Avatar/Equippables/PumpShotgun"; 
        }

        private void OnGUI()
        {
            if (!SurvivalEnabled && _hudAlpha <= 0) return;
            if (_waveStyle == null) { _waveStyle = new GUIStyle(); _waveStyle.richText = true; }
            
            // Apply Global Alpha
            GUI.color = new Color(1, 1, 1, _hudAlpha);

            // Centralized Style Initialization
            _waveStyle.alignment = TextAnchor.MiddleCenter;
            _waveStyle.fontStyle = FontStyle.Bold;

            if (_isLost)
            {
                GUI.color = Color.white; // Reset for death screen
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), ""); // Dim background
                
                _waveStyle.fontSize = 72;
                _waveStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 150, 600, 100), "YOU DIED", _waveStyle);

                _waveStyle.fontSize = 32;
                _waveStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 50, 600, 50), $"Survived until Wave: {_waveManager.CurrentWave}", _waveStyle);

                if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 60), "RESPAWN"))
                {
                    RespawnPlayer();
                }
                return;
            }

            float centerX = Screen.width / 2;
            float topY = 40;

            if (_isInitialPrep)
            {
                _waveStyle.fontSize = 48;
                DrawShadowedLabel(new Rect(centerX - 400, topY, 800, 60), "PREPARATION PHASE", _waveStyle, new Color(1, 0.92f, 0.016f, _hudAlpha));
                
                // Pulse countdown
                float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.1f;
                _waveStyle.fontSize = Mathf.RoundToInt(64 * pulse);
                DrawShadowedLabel(new Rect(centerX - 400, topY + 60, 800, 80), Mathf.CeilToInt(_initialPrepTimeRemaining).ToString(), _waveStyle, new Color(1, 1, 1, _hudAlpha));

                _waveStyle.fontSize = 20;
                _waveStyle.normal.textColor = new Color(1, 1, 1, _hudAlpha);
                GUI.Label(new Rect(centerX - 400, topY + 140, 800, 30), "Press <color=yellow>[ENTER]</color> to start now", _waveStyle);
                return;
            }

            if (_waveManager.CurrentWave == 0) return;
            
            // Draw Main HUD Container (More compact)
            float containerWidth = 350;
            float containerHeight = (_currentModifier != EWaveModifier.None) ? 110 : 85;
            Rect containerRect = new Rect(centerX - (containerWidth / 2), topY - 10, containerWidth, containerHeight);
            
            GUI.color = new Color(0, 0, 0, 0.7f * _hudAlpha);
            GUI.DrawTexture(containerRect, Texture2D.whiteTexture);
            GUI.color = new Color(1, 1, 1, _hudAlpha);

            // 1. Wave Counter (Slightly smaller, now ROMAN)
            _waveStyle.fontSize = 42;
            DrawShadowedLabel(new Rect(centerX - 400, topY, 800, 50), $"WAVE {ToRoman(_waveManager.CurrentWave)}", _waveStyle, new Color(1, 0, 0, _hudAlpha));

            // 2. Enemies Remaining Progress Bar (Fades out when Boss HUD is active)
            float normalHudAlpha = _hudAlpha * (1f - _bossHudAlpha);
            if (normalHudAlpha > 0.05f)
            {
                int total = _waveManager.TotalEnemiesForWave;
                int currentKills = _waveManager.KilledEnemies;
                // Calculate enemies ALIVE (active + to be spawned)
                // Actually, let's show progress towards QUOTA.
                // Bar fills up as you kill.
                float progress = (float)currentKills / total;
                
                float barWidth = 300;
                float barHeight = 6;
                Rect barRect = new Rect(centerX - (barWidth / 2), topY + 50, barWidth, barHeight);
                
                GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f * normalHudAlpha);
                GUI.DrawTexture(barRect, Texture2D.whiteTexture);
                
                GUI.color = new Color(1, 0, 0, normalHudAlpha);
                GUI.DrawTexture(new Rect(barRect.x, barRect.y, barWidth * progress, barHeight), Texture2D.whiteTexture); 
                
                GUI.color = new Color(1, 1, 1, normalHudAlpha);
                _waveStyle.fontSize = 14;
                GUI.Label(new Rect(centerX - 200, topY + 58, 400, 18), $"KILLS: {currentKills} / {total}", _waveStyle);
            }

            // 3. Modifiers (More compact)
            if (_currentModifier != EWaveModifier.None)
            {
                _waveStyle.fontSize = 18;
                DrawShadowedLabel(new Rect(centerX - 400, topY + 78, 800, 25), _currentModifier.ToString().ToUpper(), _waveStyle, new Color(1, 0.6f, 0, _hudAlpha));
            }

            // 4. Elden Ring Style Boss HUD
            if (_bossHudAlpha > 0.01f)
            {
                var activeBosses = GetActiveBosses();
                if (activeBosses.Count > 0)
                {
                    var boss = activeBosses[0];
                    float bossHealth = boss.Health.Health;
                    float bossMax = boss.Health.MaxHealth;
                    float bossPercent = Mathf.Clamp01(bossHealth / bossMax);
                    string bossName = boss.fullName.ToUpper();
                    if (bossName.Contains("SEWER GOBLIN")) bossName = "THE SEWER GOBLIN";

                    float bossHudY = Screen.height - 100;
                    float bossBarWidth = 800; // Souls bars are very long
                    float bossBarHeight = 4;  // Souls bars are very thin

                    // Boss Title (Elegantly positioned)
                    _waveStyle.fontSize = 22;
                    _waveStyle.alignment = TextAnchor.LowerLeft;
                    _waveStyle.normal.textColor = new Color(1, 1, 1, _bossHudAlpha);
                    GUI.Label(new Rect(centerX - (bossBarWidth / 2), bossHudY - 28, bossBarWidth, 25), bossName, _waveStyle);

                    // Boss Health Bar Background (Dark and thin)
                    GUI.color = new Color(0, 0, 0, 0.9f * _bossHudAlpha);
                    GUI.DrawTexture(new Rect(centerX - (bossBarWidth / 2) - 1, bossHudY - 1, bossBarWidth + 2, bossBarHeight + 2), Texture2D.whiteTexture);

                    // Health Fill (Deep Red)
                    GUI.color = new Color(0.5f, 0, 0, _bossHudAlpha);
                    GUI.DrawTexture(new Rect(centerX - (bossBarWidth / 2), bossHudY, bossBarWidth * bossPercent, bossBarHeight), Texture2D.whiteTexture);
                    
                    // Golden highlight/border (Souls detail)
                    GUI.color = new Color(0.8f, 0.7f, 0.2f, 0.3f * _bossHudAlpha);
                    DrawFrame(new Rect(centerX - (bossBarWidth / 2) - 2, bossHudY - 2, bossBarWidth + 4, bossBarHeight + 4), 1);

                                        GUI.color = Color.white;
                                        _waveStyle.alignment = TextAnchor.MiddleCenter;
                                    }
                                }
                    
                                // 5. Wave Transition Countdown
                                if (_isWaitingForNextWave && _nextWaveTimeRemaining > 0)
                                {
                                    float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.1f;
                                    _waveStyle.fontSize = Mathf.RoundToInt(28 * pulse);
                                    DrawShadowedLabel(new Rect(centerX - 400, Screen.height - 180, 800, 40), $"NEXT WAVE IN: {Mathf.CeilToInt(_nextWaveTimeRemaining)}", _waveStyle, new Color(1, 0.92f, 0.016f, _hudAlpha));
                                }
                            }
                    
                            private void DrawShadowedLabel(Rect rect, string text, GUIStyle style, Color color)        {
            style.normal.textColor = new Color(0, 0, 0, color.a * 0.8f);
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        private void DrawFrame(Rect rect, int width)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - width, rect.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x + rect.width - width, rect.y, width, rect.height), Texture2D.whiteTexture);
        }
    }
}