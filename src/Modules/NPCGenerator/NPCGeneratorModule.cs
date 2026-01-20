using WindyFramework.Core;
using MelonLoader;

namespace WindyFramework.Modules.NPCGenerator
{
    public class NPCGeneratorModule : IWindyModule
    {
        public void OnInitialize()
        {
            MelonLogger.Msg("[NPCGenerator] Module Initialized.");
        }

        public void OnUpdate()
        {
            NPCGeneratorUI.Instance.OnUpdate();
        }

        public void OnFixedUpdate() { }

        public void OnLateUpdate() { }

        public void OnGUI()
        {
            NPCGeneratorUI.Instance.OnGUI();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName) { }

        public void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        public void OnPreferencesSaved() { }
    }
}
