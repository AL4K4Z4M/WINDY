using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;

namespace WindyFramework.Core
{
    public class ModuleManager
    {
        private static ModuleManager _instance;
        public static ModuleManager Instance => _instance ?? (_instance = new ModuleManager());

        private readonly List<IWindyModule> _modules = new List<IWindyModule>();

        public void RegisterModule(IWindyModule module)
        {
            if (!_modules.Contains(module))
            {
                _modules.Add(module);
                MelonLogger.Msg($"[WindyFW] Registered module: {module.GetType().Name}");
            }
        }

        public void OnInitialize()
        {
            // Auto-discover modules in the current assembly
            DiscoverModules();

            foreach (var module in _modules)
            {
                try
                {
                    module.OnInitialize();
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"[WindyFW] Failed to initialize module {module.GetType().Name}: {e}");
                }
            }
        }

        private void DiscoverModules()
        {
            MelonLogger.Msg("[WindyFW] Starting Module Discovery...");
            var interfaceType = typeof(IWindyModule);
            var moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            MelonLogger.Msg($"[WindyFW] Found {moduleTypes.Count()} potential modules.");

            foreach (var type in moduleTypes)
            {
                MelonLogger.Msg($"[WindyFW] Inspecting module: {type.Name}");

                // Check Configuration
                if (type.Name.Contains("Survival") && !WindyConfig.EnableSurvival.Value)
                {
                    MelonLogger.Msg($"[WindyFW] Skipping {type.Name} (Disabled in Config)");
                    continue;
                }
                if (type.Name.Contains("WorldEditor") && !WindyConfig.EnableWorldEditor.Value)
                {
                    MelonLogger.Msg($"[WindyFW] Skipping {type.Name} (Disabled in Config)");
                    continue;
                }
                if (type.Name.Contains("UIModule") && !WindyConfig.EnableUI.Value)
                {
                    MelonLogger.Msg($"[WindyFW] Skipping {type.Name} (Disabled in Config)");
                    continue;
                }
                if (type.Name.Contains("NPCGenerator") && !WindyConfig.EnableNPCGenerator.Value)
                {
                    MelonLogger.Msg($"[WindyFW] Skipping {type.Name} (Disabled in Config)");
                    continue;
                }

                try
                {
                    var module = (IWindyModule)Activator.CreateInstance(type);
                    RegisterModule(module);
                    MelonLogger.Msg($"[WindyFW] Instantiated {type.Name}");
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"[WindyFW] Failed to instantiate module {type.Name}: {e}");
                }
            }
        }

        public void OnUpdate()
        {
            foreach (var module in _modules) module.OnUpdate();
        }

        public void OnFixedUpdate()
        {
            foreach (var module in _modules) module.OnFixedUpdate();
        }

        public void OnLateUpdate()
        {
            foreach (var module in _modules) module.OnLateUpdate();
        }

        public void OnGUI()
        {
            foreach (var module in _modules) module.OnGUI();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var module in _modules) module.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var module in _modules) module.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public void OnPreferencesSaved()
        {
            foreach (var module in _modules) module.OnPreferencesSaved();
        }
    }
}
