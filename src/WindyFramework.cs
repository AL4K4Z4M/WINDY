using MelonLoader;
using UnityEngine;
using WindyFramework.Core;

[assembly: MelonInfo(typeof(WindyFramework.WindyFrameworkMod), "WindyFramework", "2.0.0", "Windy")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace WindyFramework
{
    public class WindyFrameworkMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("WindyFramework v2.0.0 Initializing...");
            
            // Initialize Config
            WindyConfig.Initialize();

            // Initialize Module Manager
            ModuleManager.Instance.OnInitialize();
            
            MelonLogger.Msg("WindyFramework Initialized.");
        }

        public override void OnUpdate()
        {
            ModuleManager.Instance.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            ModuleManager.Instance.OnFixedUpdate();
        }

        public override void OnLateUpdate()
        {
            ModuleManager.Instance.OnLateUpdate();
        }

        public override void OnGUI()
        {
            ModuleManager.Instance.OnGUI();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ModuleManager.Instance.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ModuleManager.Instance.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public override void OnPreferencesSaved()
        {
            ModuleManager.Instance.OnPreferencesSaved();
        }
    }
}
