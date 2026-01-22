using WindyFramework.Core;
using MelonLoader;

namespace WindyFramework.Modules.Enemies
{
    public class EnemiesModule : IWindyModule
    {
        public void OnInitialize()
        {
            MelonLogger.Msg("[Enemies] Module Initialized.");
        }

        public void OnUpdate() { }

        public void OnFixedUpdate() { }

        public void OnLateUpdate() { }

        public void OnGUI() { }

        public void OnSceneWasLoaded(int buildIndex, string sceneName) { }

        public void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        public void OnPreferencesSaved() { }
    }
}
