using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zordon.ScheduleI.WorldEditor.Models
{
    [Serializable]
    public class ObjectPatch
    {
        public string Name;
        public string ScenePath;
        public Vector3 Position;
        public Vector3 Rotation; // Euler angles for easier JSON reading
        public Vector3 Scale;
        public bool IsActive = true;
        public string SourceItemID; // For spawned objects

        public ObjectPatch() { }

        public ObjectPatch(GameObject obj, string path, string sourceID = null)
        {
            Name = obj.name;
            ScenePath = path;
            Position = obj.transform.position;
            Rotation = obj.transform.eulerAngles;
            Scale = obj.transform.localScale;
            IsActive = obj.activeSelf;
            SourceItemID = sourceID;
        }
    }

    [Serializable]
    public class WorldPatchData
    {
        public List<ObjectPatch> Patches = new List<ObjectPatch>();

        public WorldPatchData() { }
    }
}
