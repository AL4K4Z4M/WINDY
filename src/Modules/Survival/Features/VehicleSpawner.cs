using UnityEngine;
using MelonLoader;
using ScheduleOne.Vehicles;
using ScheduleOne.PlayerScripts;

namespace Zordon.ScheduleI.Survival.Features
{
    public class VehicleSpawner
    {
        private static VehicleSpawner _instance;
        public static VehicleSpawner Instance => _instance ?? (_instance = new VehicleSpawner());

        public void SpawnVehicle(string vehicleCode, bool playerOwned = true)
        {
            if (Player.Local == null)
            {
                MelonLogger.Warning("Cannot spawn vehicle: Player.Local is null.");
                return;
            }

            var vehicleManager = VehicleManager.Instance;
            if (vehicleManager == null)
            {
                MelonLogger.Error("VehicleManager.Instance is null!");
                return;
            }

            Vector3 spawnPos = Player.Local.transform.position + Player.Local.transform.forward * 5f;
            Quaternion spawnRot = Player.Local.transform.rotation;

            try
            {
                var vehicle = vehicleManager.SpawnAndReturnVehicle(vehicleCode, spawnPos, spawnRot, playerOwned);
                if (vehicle != null)
                {
                    MelonLogger.Msg($"Successfully spawned vehicle: {vehicleCode}");
                }
                else
                {
                    MelonLogger.Warning($"Failed to spawn vehicle with code: {vehicleCode}. Check if the code is correct.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Exception while spawning vehicle '{vehicleCode}': {ex}");
            }
        }
    }
}
