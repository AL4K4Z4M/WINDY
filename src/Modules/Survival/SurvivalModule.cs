using WindyFramework.Core;
using MelonLoader;
using Zordon.ScheduleI.Survival.Features;

namespace Zordon.ScheduleI.Survival
{
    public class SurvivalModule : IWindyModule
    {
        public void OnInitialize()
        {
            SurvivalManager.Instance.Initialize();
            MelonLogger.Msg("[Survival] Module Initialized.");
        }

        public void OnUpdate()
        {
            SurvivalManager.Instance.OnUpdate();
            DebugMenu.Instance.OnUpdate();
        }

        public void OnFixedUpdate() { }

        public void OnLateUpdate() { }

        public void OnGUI()
        {
            SurvivalManager.Instance.OnGUI();
            DebugMenu.Instance.OnGUI();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName) { }

        public void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        public void OnPreferencesSaved() { }
    }
}
