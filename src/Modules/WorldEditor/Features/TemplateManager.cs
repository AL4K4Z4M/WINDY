using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Zordon.ScheduleI.WorldEditor.Models;

namespace Zordon.ScheduleI.WorldEditor.Features
{
    public class TemplateManager
    {
        private static TemplateManager _instance;
        public static TemplateManager Instance => _instance ?? (_instance = new TemplateManager());

        private string _savePath;
        public List<ObjectTemplate> Templates { get; private set; } = new List<ObjectTemplate>();

        public TemplateManager()
        {
            _savePath = Path.Combine(MelonEnvironment.UserDataDirectory, "WorldEditor", "templates.json");
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            if (!File.Exists(_savePath)) return;

            try
            {
                string json = File.ReadAllText(_savePath);
                var data = UnityEngine.JsonUtility.FromJson<TemplateList>(json);
                if (data != null && data.Templates != null)
                {
                    Templates = data.Templates;
                    MelonLogger.Msg($"Loaded {Templates.Count} object templates.");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to load templates: {e.Message}");
            }
        }

        public void SaveTemplates()
        {
            try
            {
                var data = new TemplateList { Templates = Templates };
                string json = UnityEngine.JsonUtility.ToJson(data, true);
                
                string dir = Path.GetDirectoryName(_savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                
                File.WriteAllText(_savePath, json);
                MelonLogger.Msg("Templates saved.");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to save templates: {e.Message}");
            }
        }

        public void AddTemplate(string name, string sourceID, Vector3 scale)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sourceID)) return;

            // Overwrite if exists
            var existing = Templates.Find(t => t.Name == name);
            if (existing != null)
            {
                existing.SourceItemID = sourceID;
                existing.Scale = scale;
            }
            else
            {
                Templates.Add(new ObjectTemplate(name, sourceID, scale));
            }
            SaveTemplates();
        }

        public void DeleteTemplate(ObjectTemplate template)
        {
            if (Templates.Contains(template))
            {
                Templates.Remove(template);
                SaveTemplates();
            }
        }
    }
}
