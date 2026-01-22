using UnityEngine;
using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using ScheduleOne.PlayerScripts;

namespace Zordon.ScheduleI.Survival.Features
{
    public class LocationLogger
    {
        private static LocationLogger _instance;
        public static LocationLogger Instance => _instance ?? (_instance = new LocationLogger());

        private System.Collections.Generic.List<GameObject> _activeMarkers = new System.Collections.Generic.List<GameObject>();

        public void ToggleMarkers()
        {
            if (_activeMarkers.Count > 0)
            {
                ClearMarkers();
                MelonLogger.Msg("[LocationLogger] Markers Hidden.");
            }
            else
            {
                InitializeMarkers("Survival_SpawnPoints.txt", Color.green);
                InitializeMarkers("Survival_EnemySpawnPoints.txt", Color.red);
                InitializeMarkers("Survival_PlayerSpawnPoints.txt", Color.cyan);
                MelonLogger.Msg("[LocationLogger] Markers Shown.");
            }
        }

        private void InitializeMarkers(string fileName, Color color)
        {
            string path = Path.Combine(SurvivalController.Instance.DataPath, fileName);
            if (!File.Exists(path)) return;

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');
                if (parts.Length < 2) continue;
                string[] posParts = parts[1].Split(',');
                if (posParts.Length == 3)
                {
                    Vector3 pos = new Vector3(float.Parse(posParts[0]), float.Parse(posParts[1]), float.Parse(posParts[2]));
                    CreateMarker(pos, color);
                }
            }
        }

        private void CreateMarker(Vector3 position, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Survival_SpawnMarker";
            Object.Destroy(marker.GetComponent<CapsuleCollider>()); 
            
            // 2000m tall pillar reaching the sky
            marker.transform.position = position + Vector3.up * 1000f; 
            marker.transform.localScale = new Vector3(0.3f, 1000f, 0.3f); 

            var renderer = marker.GetComponent<Renderer>();
            // Use Unlit/Color to ensure it glows and ignores world lighting/shadows
            renderer.material.shader = Shader.Find("Unlit/Color");
            renderer.material.color = color;

            _activeMarkers.Add(marker);
        }

        private void ClearMarkers()
        {
            foreach (var m in _activeMarkers) if (m != null) Object.Destroy(m);
            _activeMarkers.Clear();
        }

        public void LogLocation(string fileName, Color color)
        {
            if (Player.Local == null)
            {
                MelonLogger.Warning("Cannot log location: Player.Local is null.");
                return;
            }

            Vector3 pos = Player.Local.transform.position;
            Vector3 rot = Player.Local.transform.eulerAngles;

            string logEntry = $"SpawnPoint_{System.DateTime.Now:HHmmss} | {pos.x:F3}, {pos.y:F3}, {pos.z:F3} | {rot.x:F3}, {rot.y:F3}, {rot.z:F3}";
            string fullPath = Path.Combine(SurvivalController.Instance.DataPath, fileName);

            try
            {
                File.AppendAllLines(fullPath, new[] { logEntry });
                MelonLogger.Msg($"[LocationLogger] Saved coordinates to: {fullPath}");
                CreateMarker(pos, color); // Add marker immediately
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to write to spawn point log: {ex}");
            }
        }
        
        // Backward compatibility for DebugMenu
        public void LogCurrentLocation()
        {
            LogLocation("Survival_SpawnPoints.txt", Color.green);
        }
    }
}
