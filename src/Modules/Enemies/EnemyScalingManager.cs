using UnityEngine;

namespace WindyFramework.Modules.Enemies
{
    public static class EnemyScalingManager
    {
        public const float STAT_INCREMENT_PER_WAVE = 0.1f; // 10% buff per wave
        public const float SPEED_INCREMENT_PER_WAVE = 0.05f; // 5% speed per wave
        public const float CHANCE_INCREMENT_PER_WAVE = 0.025f; // 2.5% mutation chance per wave

        public static float GetDifficultyMultiplier(int wave)
        {
            if (wave < 1) return 1.0f;
            return 1.0f + (wave - 1) * STAT_INCREMENT_PER_WAVE;
        }

        public static float GetSpeedMultiplier(int wave)
        {
            if (wave < 1) return 1.0f;
            return 1.0f + (wave - 1) * SPEED_INCREMENT_PER_WAVE;
        }

        public static float GetMutationChance(int wave)
        {
            float chance = 0.05f + (wave - 1) * CHANCE_INCREMENT_PER_WAVE;
            return Mathf.Clamp(chance, 0.05f, 0.5f);
        }

        public static EnemyStats GenerateStatsForWave(int wave)
        {
            EnemyStats stats = new EnemyStats();
            float diffMult = GetDifficultyMultiplier(wave);
            float speedMult = GetSpeedMultiplier(wave);

            // Base scaling + randomization for variety
            stats.BaseHealth = 100f * diffMult * Random.Range(0.9f, 1.2f);
            stats.MoveSpeed = 7f * speedMult * Random.Range(0.8f, 1.3f);
            stats.Damage = 20f * diffMult * Random.Range(1.0f, 1.4f);
            stats.AttackSpeed = 1f * diffMult * Random.Range(0.9f, 1.1f);

            return stats;
        }
    }
}
