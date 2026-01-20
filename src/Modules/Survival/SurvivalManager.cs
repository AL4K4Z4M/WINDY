using UnityEngine;
using MelonLoader;
using Zordon.ScheduleI.Survival.Features;

namespace Zordon.ScheduleI.Survival
{
    public class SurvivalManager
    {
        private static SurvivalManager _instance;
        public static SurvivalManager Instance => _instance ?? (_instance = new SurvivalManager());

        public void Initialize()
        {
            // Initial initialization logic if needed
        }

        public void OnUpdate()
        {
            // Toggle Survival Mode directly
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                var controller = SurvivalController.Instance;
                controller.SurvivalEnabled = !controller.SurvivalEnabled;
                
                if (controller.SurvivalEnabled)
                {
                    MelonLogger.Msg("[Survival] Mode ENABLED via Insert.");
                    if (!controller.IsWaveActive) controller.StartNewWave();
                }
                else
                {
                    MelonLogger.Msg("[Survival] Mode DISABLED via Insert.");
                    controller.StopWaves();
                }
            }
        }

        public void OnGUI()
        {
            // GUI hooks are handled via DebugMenu proxy in Main.cs
        }
    }
}
