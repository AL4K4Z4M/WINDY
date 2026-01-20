using System;
using UnityEngine;

namespace Zordon.ScheduleI.WorldEditor.Models
{
    [Serializable]
    public class ObjectTemplate
    {
        public string Name;
        public string SourceItemID; // The ID used to spawn the base object (e.g., "VEH_shitbox", "coffeetable")
        public Vector3 Scale;
        
        // We generally don't save position/rotation for templates as they are placed new
        // But we could save a "default rotation" if needed. For now, Scale is the most important custom attribute.

        public ObjectTemplate() { }

        public ObjectTemplate(string name, string sourceID, Vector3 scale)
        {
            Name = name;
            SourceItemID = sourceID;
            Scale = scale;
        }
    }

    [Serializable]
    public class TemplateList
    {
        public System.Collections.Generic.List<ObjectTemplate> Templates = new System.Collections.Generic.List<ObjectTemplate>();
    }
}
