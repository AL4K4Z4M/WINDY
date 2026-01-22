using UnityEngine;

namespace Zordon.ScheduleI.Survival.Features
{
    public class WaveManager
    {
        public int CurrentWave { get; private set; }
        public int TotalEnemiesForWave { get; private set; }
        public int SpawnedEnemies { get; private set; }
        public int KilledEnemies { get; private set; }
        
        public bool IsWaveComplete => KilledEnemies >= TotalEnemiesForWave;

        public WaveManager()
        {
            CurrentWave = 0;
        }

        public void StartNextWave()
        {
            CurrentWave++;
            TotalEnemiesForWave = CalculateTotalEnemies(CurrentWave);
            SpawnedEnemies = 0;
            KilledEnemies = 0;
        }

        public void Reset()
        {
            CurrentWave = 0;
            TotalEnemiesForWave = 0;
            SpawnedEnemies = 0;
            KilledEnemies = 0;
        }

        public bool CanSpawnMore()
        {
            return SpawnedEnemies < TotalEnemiesForWave;
        }

        public void OnEnemySpawned()
        {
            SpawnedEnemies++;
        }

        public void OnEnemyKilled()
        {
            KilledEnemies++;
        }

        public void OnSpawnRefunded()
        {
            SpawnedEnemies--;
            if (SpawnedEnemies < 0) SpawnedEnemies = 0;
        }

        public void OnWaveSkipped()
        {
            // Set spawned to total so no more enemies spawn for this wave
            SpawnedEnemies = TotalEnemiesForWave;
        }

        public bool IsBossWave => CurrentWave > 0 && CurrentWave % 5 == 0;

        private int CalculateTotalEnemies(int wave)
        {
            if (wave % 5 == 0) return 1; // Boss only wave
            
            // Slower progression: 5, 7, 9, 11, 13, 15...
            return 5 + (wave - 1) * 2;
        }

        public float GetSpawnDelay()
        {
            // Clamp delay to minimum 0.4s
            return Mathf.Max(0.4f, 1.3f - (CurrentWave * 0.1f));
        }
    }
}
