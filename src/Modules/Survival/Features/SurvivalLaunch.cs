using System.IO;
using UnityEngine;
using MelonLoader;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;

namespace Zordon.ScheduleI.Survival.Features
{
    public static class SurvivalLaunch
    {
        public const string SURVIVAL_FOLDER = "SaveGame_Survival";

        public static void StartSurvival()
        {
            try
            {
                MelonLogger.Msg("[Survival] Preparing Survival Mode...");

                // Path
                string savePath = Path.Combine(Singleton<SaveManager>.Instance.IndividualSavesContainerPath, SURVIVAL_FOLDER);

                // Wipe existing survival save to ensure fresh start
                if (Directory.Exists(savePath))
                {
                    Directory.Delete(savePath, true);
                }
                Directory.CreateDirectory(savePath);

                // Copy Default Save
                CopyDefaultSaveToFolder(savePath);

                // Write Game.json
                string gameJson = new GameData(
                    seed: UnityEngine.Random.Range(0, int.MaxValue), 
                    organisationName: "Survival Corp", 
                    settings: new GameSettings()
                ).GetJson();
                File.WriteAllText(Path.Combine(savePath, "Game.json"), gameJson);

                // Create MetaData
                MetaData meta = new MetaData(
                    new DateTimeData(System.DateTime.Now), 
                    new DateTimeData(System.DateTime.Now), 
                    Application.version, 
                    Application.version, 
                    playTutorial: false
                );

                // Write Metadata.json
                File.WriteAllText(Path.Combine(savePath, "Metadata.json"), meta.GetJson());

                // Construct SaveInfo manually
                // We use -1 for index to indicate custom/survival
                SaveInfo info = new SaveInfo(
                    savePath, 
                    -1, 
                    "Survival Mode", 
                    System.DateTime.Now, 
                    System.DateTime.Now, 
                    0f, 
                    Application.version, 
                    meta 
                );
                
                // Flag Survival Controller
                if (SurvivalController.Instance != null)
                {
                    SurvivalController.Instance.IsSurvivalPending = true;
                    SurvivalController.Instance.SurvivalEnabled = false;
                }
                
                Singleton<LoadManager>.Instance.StartGame(info, allowLoadStacking: false, allowSaveBackup: false);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[Survival] Start Failed: {ex}");
            }
        }

        public static Vector3 GetRandomPlayerSpawn()
        {
            string path = Path.Combine(SurvivalController.Instance.DataPath, "Survival_PlayerSpawnPoints.txt");
            if (!File.Exists(path)) return Vector3.zero;

            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0) return Vector3.zero;

            var validPoints = new System.Collections.Generic.List<Vector3>();
            foreach(var line in lines) {
                var parts = line.Split('|');
                if (parts.Length > 1) {
                    var p = parts[1].Split(',');
                    if(p.Length == 3) {
                        validPoints.Add(new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2])));
                    }
                }
            }

            if (validPoints.Count > 0)
                return validPoints[UnityEngine.Random.Range(0, validPoints.Count)];
            
            return Vector3.zero;
        }

        private static void CopyDefaultSaveToFolder(string folderPath)
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, "DefaultSave");
            if (!Directory.Exists(sourcePath))
            {
                MelonLogger.Error($"[Survival] DefaultSave not found at {sourcePath}");
                return;
            }
            CopyFolder(sourcePath, folderPath);
        }

        private static void CopyFolder(string source, string target)
        {
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
            {
                string name = Path.GetFileName(file);
                if (name.EndsWith(".meta")) continue;
                File.Copy(file, Path.Combine(target, name), true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                string dirName = new DirectoryInfo(dir).Name;
                if (dirName == "NPCs" || dirName == "OwnedVehicles") continue; 

                CopyFolder(dir, Path.Combine(target, dirName));
            }
        }
    }
}