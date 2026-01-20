using WindyFramework.Core;
using MelonLoader;
using UnityEngine;
using Zordon.ScheduleI.WorldEditor.Features;

namespace Zordon.ScheduleI.WorldEditor
{
    public class WorldEditorModule : IWindyModule
    {
        public void OnInitialize()
        {
            MelonLogger.Msg("[WorldEditor] Module Initialized.");
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1)) // Toggle Editor
            {
                EditorController.Instance.ToggleEditor();
            }
        }

        public void OnFixedUpdate() { }

        public void OnLateUpdate() { }

        public void OnGUI() { }

        public void OnSceneWasLoaded(int buildIndex, string sceneName) { }

        public void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        public void OnPreferencesSaved() { }
    }
}
