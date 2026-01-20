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
using System.IO;

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

        public int CurrentWave { get; private set; } = 0;
        public bool IsWaveActive { get; private set; } = false;
        public bool SurvivalEnabled { get; set; } = false;
        
        private bool _isLost = false;
        public bool IsLostState => _isLost;

        private bool _isWaitingForNextWave = false;
        private int _totalEnemiesForWave = 0;
        private int _spawnedEnemiesForWave = 0;
        
        private const int MAX_ACTIVE_GOONS = 5;
        private const int MAX_ACTIVE_COPS = 5;

        private List<CartelGoon> _activeGoons = new List<CartelGoon>();
        private List<PoliceOfficer> _activeCops = new List<PoliceOfficer>();
        private List<NPC> _activeCivilians = new List<NPC>();
        private List<ScheduleOne.Vehicles.LandVehicle> _activeVans = new List<ScheduleOne.Vehicles.LandVehicle>();
        
        private class SpawnPoint
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float LastUsedTime;
        }
        private List<SpawnPoint> _customSpawnPoints = new List<SpawnPoint>();

        private struct SavedItem { public int Index; public ItemInstance Instance; }
        private List<SavedItem> _savedInventory = new List<SavedItem>();
        private float _savedCash = 0f;
        private float _savedBankBalance = 0f;

        private GUIStyle _waveStyle;
        private float _spawnTimer = 0f;
        private float _nextWaveTimeRemaining = 0f;

        private void ClearWorldOfNPCs()
        {
            MelonLogger.Msg("[Survival] Purging the world of all non-survival entities...");

            // 1. Clear Goons
            var goonPool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance?.GoonPool;
            if (goonPool != null)
            {
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

            // 2. Clear Cops
            var allCops = GameObject.FindObjectsOfType<PoliceOfficer>();
            foreach (var cop in allCops)
            {
                if (cop != null) cop.Deactivate();
            }

            // 3. Deactivate all ambient NPCs
            foreach (var npc in NPCManager.NPCRegistry)
            {
                if (npc != null && !(npc is CartelGoon) && !(npc is PoliceOfficer))
                {
                    npc.gameObject.SetActive(false);
                }
            }
            
            ClearVans();
        }

        private void LoadCustomSpawnPoints()
        {
            _customSpawnPoints.Clear();
            string path = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "Survival_SpawnPoints.txt");
            if (!File.Exists(path)) return;
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length < 3) continue;
                    _customSpawnPoints.Add(new SpawnPoint { 
                        Position = ParseVector(parts[1]), 
                        Rotation = Quaternion.Euler(ParseVector(parts[2])),
                        LastUsedTime = -100f
                    });
                }
            } catch {}
        }

        private Vector3 ParseVector(string s) { string[] p = s.Split(','); return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2])); }

        private void StartArmingPhase()
        {
            if (Player.Local == null) return;
            LoadCustomSpawnPoints();
            if (_customSpawnPoints.Count == 0) return;
            string[] weaponIDs = { "baseballbat", "machete", "m1911", "revolver" };
            int vanCount = Mathf.Clamp(_customSpawnPoints.Count / 3, 2, 10);
            for (int i = 0; i < _customSpawnPoints.Count; i++) { var temp = _customSpawnPoints[i]; int r = Random.Range(i, _customSpawnPoints.Count); _customSpawnPoints[i] = _customSpawnPoints[r]; _customSpawnPoints[r] = temp; }
            for (int i = 0; i < vanCount; i++) {
                var p = _customSpawnPoints[i];
                var van = ScheduleOne.Vehicles.VehicleManager.Instance.SpawnAndReturnVehicle("Veeper", p.Position + Vector3.up, p.Rotation, false);
                if (van) { InitializeArmoryVan(van, weaponIDs); _activeVans.Add(van); p.LastUsedTime = Time.time + 9999f; }
            }
        }

        private void InitializeArmoryVan(ScheduleOne.Vehicles.LandVehicle van, string[] weapons)
        {
            var storage = van.GetComponent<StorageEntity>();
            if (!storage) return;
            storage.AccessSettings = StorageEntity.EAccessSettings.Full;
            storage.SlotCount = 10;
            storage.ItemSlots = new List<ItemSlot>();
            for (int i = 0; i < 10; i++) { var slot = new ItemSlot(false); slot.SetSlotOwner(storage); storage.ItemSlots.Add(slot); }
            for (int i = 0; i < 6; i++) { ItemDefinition def = ScheduleOne.Registry.GetItem(weapons[Random.Range(0, weapons.Length)]); if (def != null) storage.InsertItem(def.GetDefaultInstance()); }
        }

        private Vector3 GetValidSpawnPosition()
        {
            if (Player.Local == null || _customSpawnPoints.Count == 0) return Vector3.zero;
            Vector3 pPos = Player.Local.transform.position;
            List<SpawnPoint> valid = new List<SpawnPoint>();
            List<SpawnPoint> spread = new List<SpawnPoint>();
            foreach(var p in _customSpawnPoints) {
                float d = Vector3.Distance(p.Position, pPos);
                if (d < 30f || d > 120f) continue;
                valid.Add(p);
                if (Time.time - p.LastUsedTime > 10f) spread.Add(p);
            }
            var final = spread.Count > 0 ? spread : valid;
            if (final.Count > 0) { var c = final[Random.Range(0, final.Count)]; c.LastUsedTime = Time.time; return c.Position; }
            return Vector3.zero;
        }

        public void StartNewWave()
        {
            if (IsWaveActive || !SurvivalEnabled) return;
            _isLost = false;
            if (CurrentWave == 0) 
            {
                CaptureInventory();
                ClearWorldOfNPCs();
            }
            ClearVans();
            StartArmingPhase();
            CurrentWave++;
            _totalEnemiesForWave = 2 + (CurrentWave * 2);
            _spawnedEnemiesForWave = 0;
            IsWaveActive = true;
            MelonLogger.Msg($"[Survival] Wave {CurrentWave} Started. Enemies: {_totalEnemiesForWave}");
        }

        private void SpawnEnemyLogic()
        {
            bool gLimit = _activeGoons.Count < MAX_ACTIVE_GOONS;
            bool cLimit = _activeCops.Count < MAX_ACTIVE_COPS;
            bool civLimit = _activeCivilians.Count < 5;
            
            float rand = Random.value;
            // 20% Cops, 40% Goons, 40% Civilians
            if (rand < 0.2f) { if (!SpawnOneCop() && gLimit) SpawnOneGoon(); }
            else if (rand < 0.6f) { if (!SpawnOneGoon() && civLimit) SpawnOneCivilian(); }
            else { if (!SpawnOneCivilian() && gLimit) SpawnOneGoon(); }
        }

        private bool SpawnOneCivilian()
        {
            var civilians = NPCManager.NPCRegistry.FindAll(n => n != null && !n.gameObject.activeSelf && !(n is CartelGoon) && !(n is PoliceOfficer));
            if (civilians.Count == 0 || Player.Local == null) return false;

            Vector3 pos = GetValidSpawnPosition();
            if (pos == Vector3.zero) return false;

            var npc = civilians[Random.Range(0, civilians.Count)];
            npc.transform.position = pos;
            npc.gameObject.SetActive(true);
            
            if (npc.Behaviour?.CombatBehaviour != null)
            {
                npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(Player.Local.NetworkObject);
                
                // Weapon assignment for later waves
                if (CurrentWave >= 3)
                {
                    string weapon = GetWeaponForWave(CurrentWave);
                    var m = typeof(CombatBehaviour).GetMethod("RpcLogic___SetWeapon_3615296227", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    m?.Invoke(npc.Behaviour.CombatBehaviour, new object[] { weapon });
                }
            }

            _activeCivilians.Add(npc);
            _spawnedEnemiesForWave++;
            return true;
        }

        private bool SpawnOneCop()
        {
            if (Player.Local == null) return false;
            var station = PoliceStation.GetClosestPoliceStation(Player.Local.transform.position);
            if(!station || station.OfficerPool.Count == 0) return false;
            Vector3 pos = GetValidSpawnPosition(); if (pos == Vector3.zero) return false;
            PoliceOfficer cop = station.PullOfficer(); if (!cop) return false;
            cop.Movement.Warp(pos);
            if (CurrentWave < 5 && cop.BatonPrefab != null) cop.GunPrefab = cop.BatonPrefab;
            cop.BeginFootPursuit_Networked(Player.Local.PlayerCode, false);
            _activeCops.Add(cop); _spawnedEnemiesForWave++; return true;
        }

        private bool SpawnOneGoon()
        {
            var pool = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.GoonPool;
            if (pool == null || Player.Local == null) return false;
            Vector3 pos = GetValidSpawnPosition(); if (pos == Vector3.zero) return false;
            CartelGoon goon = null; for(int i=0; i<3; i++) { goon = pool.SpawnGoon(pos); if(goon != null) break; }
            if (goon) {
                if (goon.Health.IsDead) goon.Health.Revive();
                _activeGoons.Add(goon); _spawnedEnemiesForWave++; return true;
            }
            return false;
        }

        private void Update()
        {
            if (!SurvivalEnabled || _isLost) return;
            if (IsWaveActive) {
                _spawnTimer += Time.deltaTime;
                float delay = Mathf.Max(0.4f, 1.3f - (CurrentWave * 0.1f));
                if (_spawnTimer > delay && _spawnedEnemiesForWave < _totalEnemiesForWave) { _spawnTimer = 0f; SpawnEnemyLogic(); }
            }
            CleanupDead();
            if (IsWaveActive && _spawnedEnemiesForWave >= _totalEnemiesForWave && _activeGoons.Count == 0 && _activeCops.Count == 0 && _activeCivilians.Count == 0) IsWaveActive = false;
            if (SurvivalEnabled && !IsWaveActive && _activeGoons.Count == 0 && _activeCops.Count == 0 && _activeCivilians.Count == 0 && !_isWaitingForNextWave) StartCoroutine(AutoStartNextWave());
        }

        private void CleanupDead()
        {
            for (int i = _activeGoons.Count - 1; i >= 0; i--) if (!_activeGoons[i] || _activeGoons[i].Health.IsDead) { if(_activeGoons[i]) StartCoroutine(RecycleGoonRoutine(_activeGoons[i])); _activeGoons.RemoveAt(i); }
            for (int i = _activeCops.Count - 1; i >= 0; i--) if (!_activeCops[i] || _activeCops[i].Health.IsDead) { if(_activeCops[i]) StartCoroutine(RecycleCopRoutine(_activeCops[i])); _activeCops.RemoveAt(i); }
            for (int i = _activeCivilians.Count - 1; i >= 0; i--) if (!_activeCivilians[i] || _activeCivilians[i].Health.IsDead) { if(_activeCivilians[i]) _activeCivilians[i].gameObject.SetActive(false); _activeCivilians.RemoveAt(i); }
        }

        private IEnumerator RecycleGoonRoutine(CartelGoon g) { yield return new WaitForSeconds(5f); if(g) { NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.GoonPool.ReturnToPool(g); g.Despawn(); } }
        private IEnumerator RecycleCopRoutine(PoliceOfficer c) { yield return new WaitForSeconds(5f); if(c) c.Deactivate(); }
        private void ClearVans() { foreach (var v in _activeVans) if (v) { if (v.NetworkObject?.IsSpawned == true) v.NetworkObject.Despawn(); else GameObject.Destroy(v.gameObject); } _activeVans.Clear(); }
        
        private void CaptureInventory() { var inv = PlayerSingleton<PlayerInventory>.Instance; var money = NetworkSingleton<MoneyManager>.Instance; if (!inv) return; _savedInventory.Clear(); for (int i = 0; i < inv.hotbarSlots.Count; i++) if (inv.hotbarSlots[i].ItemInstance != null) _savedInventory.Add(new SavedItem { Index = i, Instance = inv.hotbarSlots[i].ItemInstance }); _savedCash = inv.cashInstance != null ? inv.cashInstance.Balance : 0; _savedBankBalance = money != null ? money.onlineBalance : 0; inv.ClearInventory(); if (inv.cashInstance != null) inv.cashInstance.SetBalance(0); if (money != null && money.onlineBalance > 0) money.CreateOnlineTransaction("Survival Fee", -money.onlineBalance, 1, "Service"); }
        private void RestoreInventory() { var inv = PlayerSingleton<PlayerInventory>.Instance; var money = NetworkSingleton<MoneyManager>.Instance; if (!inv) return; inv.ClearInventory(); foreach (var s in _savedInventory) if (s.Index < inv.hotbarSlots.Count) inv.hotbarSlots[s.Index].SetStoredItem(s.Instance); if (inv.cashInstance != null) inv.cashInstance.SetBalance(_savedCash); if (money != null) { float d = _savedBankBalance - money.onlineBalance; if (d != 0) money.CreateOnlineTransaction("Survival Refund", d, 1, "Service"); } _savedInventory.Clear(); }
        
        private IEnumerator AutoStartNextWave() { _isWaitingForNextWave = true; _nextWaveTimeRemaining = 10f; while (_nextWaveTimeRemaining > 0) { _nextWaveTimeRemaining -= Time.deltaTime; yield return null; } _isWaitingForNextWave = false; if (SurvivalEnabled) StartNewWave(); }
        public void StopWaves() { StopAllCoroutines(); IsWaveActive = false; foreach(var g in _activeGoons) if(g) g.Despawn(); foreach(var c in _activeCops) if(c) c.Deactivate(); foreach(var civ in _activeCivilians) if(civ) civ.gameObject.SetActive(false); _activeGoons.Clear(); _activeCops.Clear(); _activeCivilians.Clear(); ClearVans(); RestoreInventory(); CurrentWave = 0; }
        public void OnPlayerLost() { _isLost = true; StopWaves(); }
        private string ToRoman(int n) { if (n < 1) return ""; if (n >= 10) return "X" + ToRoman(n - 10); if (n >= 9) return "IX" + ToRoman(n - 9); if (n >= 5) return "V" + ToRoman(n - 5); if (n >= 4) return "IV" + ToRoman(n - 4); return "I" + ToRoman(n - 1); }
        private string GetWeaponForWave(int w) { if (w == 1) return "Weapons/BaseballBat/BaseballBat"; if (w == 2) return "Weapons/Machete/Machete"; if (w >= 6) return "Weapons/Shotty/PumpShotgun"; return "Weapons/M1911/M1911"; }

        private void OnGUI()
        {
            if (!SurvivalEnabled || CurrentWave == 0) return;
            if (_waveStyle == null) { _waveStyle = new GUIStyle(); _waveStyle.richText = true; }
            string wT = $"<color=red>WAVE {ToRoman(CurrentWave)}</color>";
            int rem = (_totalEnemiesForWave - _spawnedEnemiesForWave) + _activeGoons.Count + _activeCops.Count;
            string eT = $"Enemies Remaining: <color=white>{rem}</color>";
            float x = Screen.width / 2 - 200;
            _waveStyle.fontSize = 48; GUI.Label(new Rect(x + 2, 22, 400, 60), wT, _waveStyle);
            _waveStyle.fontSize = 24; GUI.Label(new Rect(x + 1, 81, 400, 40), eT, _waveStyle);
            if (_isWaitingForNextWave && _nextWaveTimeRemaining > 0) {
                string cT = $"NEXT WAVE IN: <color=yellow>{Mathf.CeilToInt(_nextWaveTimeRemaining)}</color>";
                _waveStyle.fontSize = 32; GUI.Label(new Rect(x + 2, 122, 400, 50), cT, _waveStyle);
            }
        }
    }
}