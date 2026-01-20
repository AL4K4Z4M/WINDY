using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MelonLoader;
using MelonLoader.Utils;
using ScheduleOne;
using Zordon.ScheduleI.WorldEditor.Models;

namespace Zordon.ScheduleI.WorldEditor.Features
{
    public class WorldPatchManager
    {
        private static WorldPatchManager _instance;
        public static WorldPatchManager Instance => _instance ?? (_instance = new WorldPatchManager());

        private string _mapsDir;
        private WorldPatchData _currentData = new WorldPatchData();
        
        // Track original states for Revert/Unload functionality
        private class ObjectState
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public bool IsActive;
        }
        private Dictionary<string, ObjectState> _originalStates = new Dictionary<string, ObjectState>();

        public WorldPatchManager()
        {
            _mapsDir = Path.Combine(MelonEnvironment.UserDataDirectory, "WorldEditor", "Maps");
            if (!Directory.Exists(_mapsDir)) Directory.CreateDirectory(_mapsDir);
        }

        public string[] GetAvailableMaps()
        {
            if (!Directory.Exists(_mapsDir)) return new string[0];
            return Directory.GetFiles(_mapsDir, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        public void LoadMap(string mapName)
        {
            string path = Path.Combine(_mapsDir, mapName + ".json");
            if (!File.Exists(path))
            {
                MelonLogger.Error($"Map file not found: {path}");
                return;
            }

            // First, revert current changes to ensure a clean slate
            UnloadMap();

            try
            {
                string json = File.ReadAllText(path);
                _currentData = UnityEngine.JsonUtility.FromJson<WorldPatchData>(json);
                MelonLogger.Msg($"Loaded map '{mapName}' with {_currentData.Patches.Count} patches.");
                ApplyPatches();
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to load map: {e.Message}");
                _currentData = new WorldPatchData();
            }
        }

        public void SaveMap(string mapName)
        {
            try
            {
                string path = Path.Combine(_mapsDir, mapName + ".json");
                string json = UnityEngine.JsonUtility.ToJson(_currentData, true);
                File.WriteAllText(path, json);
                MelonLogger.Msg($"Map '{mapName}' saved.");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to save map: {e.Message}");
            }
        }

        public void UnloadMap()
        {
            // 1. Destroy spawned objects
            GameObject container = GameObject.Find("WorldEditor_SpawnedObjects");
            if (container != null)
            {
                UnityEngine.Object.Destroy(container);
            }

            // 2. Revert modified objects to original state
            foreach (var kvp in _originalStates)
            {
                GameObject obj = GameObject.Find(kvp.Key);
                if (obj != null)
                {
                    obj.transform.position = kvp.Value.Position;
                    obj.transform.eulerAngles = kvp.Value.Rotation;
                    obj.transform.localScale = kvp.Value.Scale;
                    obj.SetActive(kvp.Value.IsActive);
                }
            }
            
            // Clear tracking data
            _originalStates.Clear();
            _currentData = new WorldPatchData();
            
            MelonLogger.Msg("Map unloaded. Reverted to Official State.");
        }

        public void ApplyPatches()
        {
            int appliedCount = 0;
            GameObject container = null;

            foreach (var patch in _currentData.Patches)
            {
                GameObject obj = GameObject.Find(patch.ScenePath);
                
                // If object is missing but was spawned by us, recreate it
                if (obj == null && !string.IsNullOrEmpty(patch.SourceItemID))
                {
                    if (container == null) container = GetOrCreateSpawnContainer();

                    if (patch.SourceItemID.StartsWith("VEH_"))
                    {
                        string vehCode = patch.SourceItemID.Substring(4);
                        if (ScheduleOne.Vehicles.VehicleManager.Instance != null)
                        {
                            var vehPrefab = ScheduleOne.Vehicles.VehicleManager.Instance.GetVehiclePrefab(vehCode);
                            if (vehPrefab != null)
                            {
                                obj = GameObject.Instantiate(vehPrefab.gameObject);
                                obj.name = patch.Name;
                                obj.transform.SetParent(container.transform);
                            }
                        }
                    }
                    else if (patch.SourceItemID.StartsWith("TREE_"))
                    {
                        string treeName = patch.SourceItemID.Substring(5);
                        GameObject treePrefab = GetTreePrefab(treeName);
                        if (treePrefab != null)
                        {
                            obj = GameObject.Instantiate(treePrefab);
                            obj.name = patch.Name;
                            obj.transform.SetParent(container.transform);
                        }
                    }
                    else
                    {
                        var def = Registry.GetItem<ScheduleOne.ItemFramework.BuildableItemDefinition>(patch.SourceItemID);
                        if (def != null && def.BuiltItem != null)
                        {
                            obj = GameObject.Instantiate(def.BuiltItem.transform.gameObject);
                            obj.name = patch.Name;
                            obj.transform.SetParent(container.transform);
                        }
                        else
                        {
                            // Fallback for Generic Prefabs (discovered from memory)
                            GameObject prefab = Resources.FindObjectsOfTypeAll<GameObject>()
                                .FirstOrDefault(g => g.name == patch.SourceItemID && string.IsNullOrEmpty(g.scene.name));
                            
                            if (prefab != null)
                            {
                                obj = GameObject.Instantiate(prefab);
                                obj.name = patch.Name;
                                obj.transform.SetParent(container.transform);
                            }
                        }
                    }
                }

                if (obj != null)
                {
                    // Track original state before first modification
                    CaptureOriginalState(obj, patch.ScenePath);

                    obj.transform.position = patch.Position;
                    obj.transform.eulerAngles = patch.Rotation;
                    obj.transform.localScale = patch.Scale;
                    obj.SetActive(patch.IsActive);
                    appliedCount++;
                }
            }
            MelonLogger.Msg($"Applied {appliedCount} world patches.");
        }

        private void CaptureOriginalState(GameObject obj, string path)
        {
            if (!_originalStates.ContainsKey(path))
            {
                _originalStates.Add(path, new ObjectState
                {
                    Position = obj.transform.position,
                    Rotation = obj.transform.eulerAngles,
                    Scale = obj.transform.localScale,
                    IsActive = obj.activeSelf
                });
            }
        }

        private GameObject GetTreePrefab(string name)
        {
            if (Terrain.activeTerrain == null) return null;
            foreach (var proto in Terrain.activeTerrain.terrainData.treePrototypes)
            {
                if (proto.prefab.name == name) return proto.prefab;
            }
            return null;
        }

        private GameObject GetOrCreateSpawnContainer()
        {
            GameObject container = GameObject.Find("WorldEditor_SpawnedObjects");
            if (container == null) container = new GameObject("WorldEditor_SpawnedObjects");
            return container;
        }

        public void RegisterOrUpdatePatch(GameObject obj, string sourceID = null)
        {
            string path = GetScenePath(obj.transform);
            
            // Capture original state if this is the first time we touch this object
            CaptureOriginalState(obj, path);

            var existing = _currentData.Patches.Find(p => p.ScenePath == path);
            
            if (existing != null)
            {
                existing.Position = obj.transform.position;
                existing.Rotation = obj.transform.eulerAngles;
                existing.Scale = obj.transform.localScale;
                existing.IsActive = obj.activeSelf;
                if (!string.IsNullOrEmpty(sourceID)) existing.SourceItemID = sourceID;
            }
            else
            {
                _currentData.Patches.Add(new ObjectPatch(obj, path, sourceID));
            }
        }

        public ObjectPatch GetPatchForObject(GameObject obj)
        {
            if (obj == null || _currentData == null || _currentData.Patches == null) return null;
            string path = GetScenePath(obj.transform);
            return _currentData.Patches.Find(p => p.ScenePath == path);
        }

        public string GetScenePath(Transform transform)
        {
            string text = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                text = transform.name + "/" + text;
            }
            return text;
        }
    }
}