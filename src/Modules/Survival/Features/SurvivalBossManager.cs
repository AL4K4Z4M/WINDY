using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;

namespace Zordon.ScheduleI.Survival.Features
{
    public class BossNPC
    {
        public string Name;
        public float Health;
        public float Scale;
        public Color LabelColor;
        public System.Type Type;
        public string WeaponPath;

        public BossNPC(string name, float health, Color color, System.Type type = null, string weapon = "", float scale = 1.2f)
        {
            Name = name;
            Health = health;
            Scale = scale;
            LabelColor = color;
            Type = type;
            WeaponPath = weapon;
        }
    }

    public class SurvivalBossManager
    {
        private static SurvivalBossManager _instance;
        public static SurvivalBossManager Instance => _instance ??= new SurvivalBossManager();

        public List<BossNPC> BossRegistry = new List<BossNPC>
        {
            new BossNPC("Sewer Goblin", 400f, Color.green, typeof(ScheduleOne.NPCs.CharacterClasses.SewerGoblin), "", 1.6f),
            new BossNPC("Karen Kennedy", 500f, new Color(1f, 0.4f, 0f), typeof(ScheduleOne.NPCs.CharacterClasses.Karen), "", 1.3f)
        };

        public void SpawnBoss(BossNPC bossTemplate, Vector3 position, Player target, float healthOverride = -1f)
        {
            NPC npc = null;

            // 1. Try to find by Type first (for Goblins)
            if (bossTemplate.Type != null)
            {
                npc = NPCManager.NPCRegistry.Find(n => bossTemplate.Type.IsInstanceOfType(n));
            }
            
            // 2. Fallback to Name (for Karen)
            if (npc == null)
            {
                npc = NPCManager.NPCRegistry.Find(n => n.fullName != null && n.fullName.ToLower().Contains(bossTemplate.Name.ToLower()));
            }

            if (npc == null)
            {
                MelonLogger.Error($"[BossManager] Could not find NPC instance for boss: {bossTemplate.Name}");
                return;
            }

            MelonLogger.Msg($"[BossManager] Spawning Boss: {bossTemplate.Name} at {position}");

            npc.gameObject.SetActive(true);

            // Setup Position, Scaling & Visibility (Add small Y-offset to prevent ground clipping)
            Vector3 spawnPos = position + Vector3.up * 0.25f;
            if (npc.Movement != null) npc.Movement.Warp(spawnPos);
            else npc.transform.position = spawnPos;
            
            npc.transform.localScale = Vector3.one * bossTemplate.Scale;
            npc.gameObject.SetActive(true);

            // Health Setup
            float finalHealth = (healthOverride > 0) ? healthOverride : bossTemplate.Health;
            npc.Health.MaxHealth = finalHealth;
            npc.Health.RestoreHealth();
            npc.Health.Revive();

            // Label Setup
            var oldDl = npc.gameObject.GetComponent<DebugLabel>();
            if (oldDl != null) Object.Destroy(oldDl);
            var dl = npc.gameObject.AddComponent<DebugLabel>();
            dl.Setup(npc, $"BOSS: {bossTemplate.Name.ToUpper()}", bossTemplate.LabelColor);

            // Weapon Setup
            if (!string.IsNullOrEmpty(bossTemplate.WeaponPath) && npc.Avatar != null)
            {
                npc.Avatar.SetEquippable(bossTemplate.WeaponPath);
            }

            // Aggro
            if (bossTemplate.Type != null && bossTemplate.Type.Name.Contains("SewerGoblin"))
            {
                var goblin = npc as ScheduleOne.NPCs.CharacterClasses.SewerGoblin;
                goblin?.DeployToPlayer(target);
            }
            else
            {
                SurvivalController.Instance.ForceCombatState(npc, target);
            }

            // Scale Reinforcement (Prevent internal game resets)
            SurvivalController.Instance.StartCoroutine(ReinforceBossStats(npc, bossTemplate.Scale));

            SurvivalController.Instance.AddActiveCivilian(npc);
            SurvivalController.Instance.AddActiveBoss(npc);
            SurvivalController.Instance.IncrementSpawnCount();
            
            // CRITICAL: Force HUD update so it doesn't see '0 enemies' on the next frame
            SurvivalController.Instance.UpdateGroundTruthCache();
        }

        private System.Collections.IEnumerator ReinforceBossStats(NPC npc, float scale)
        {
            for (int i = 0; i < 30; i++) // Run for 6 seconds
            {
                if (npc == null || npc.Health.IsDead) yield break;
                
                // 1. Reinforce Scale & Movement State
                npc.transform.localScale = Vector3.one * scale;
                if (npc.Movement != null)
                {
                    npc.Movement.MovementSpeedScale = 1.0f;
                    npc.Movement.ResumeMovement();
                }

                // 2. Reinforce Reach (Extended Attack Distance)
                if (npc.Behaviour?.CombatBehaviour != null)
                {
                    var cb = npc.Behaviour.CombatBehaviour;
                    
                    // Boost Virtual Punch (Fallback reach)
                    if (cb.VirtualPunchWeapon != null)
                    {
                        cb.VirtualPunchWeapon.MaxUseRange = 3.5f;
                        cb.VirtualPunchWeapon.AttackRange = 3.5f;
                    }

                    // Boost Current Equipped Weapon reach
                    var weaponField = typeof(ScheduleOne.Combat.CombatBehaviour).GetField("currentWeapon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var currentWeapon = weaponField?.GetValue(cb) as ScheduleOne.AvatarFramework.Equipping.AvatarWeapon;
                    
                    if (currentWeapon != null)
                    {
                        currentWeapon.MaxUseRange = 3.5f;
                        if (currentWeapon is ScheduleOne.AvatarFramework.Equipping.AvatarMeleeWeapon melee)
                        {
                            melee.AttackRange = 3.5f;
                        }
                    }

                    // 3. Reinforce Aggression (Repeatedly force pursuit and check for manual attack)
                    var players = Player.PlayerList.FindAll(p => p != null && p.Health != null && p.Health.IsAlive);
                    if (players.Count > 0)
                    {
                        var nearest = SurvivalController.Instance.GetNearestPlayer(npc.transform.position);
                        
                        // Pursuit Refresh
                        if (i % 5 == 0) // Every 1.0 seconds
                        {
                            SurvivalController.Instance.ForceCombatState(npc, nearest);
                        }

                        // Manual Attack Trigger (Bypass AI hesitation)
                        if (Vector3.Distance(npc.transform.position, nearest.transform.position) < 3.5f)
                        {
                            var method = typeof(ScheduleOne.Combat.CombatBehaviour).GetMethod("Attack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            method?.Invoke(cb, null);
                        }
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }
        }

        public BossNPC GetRandomBoss()
        {
            return BossRegistry[Random.Range(0, BossRegistry.Count)];
        }
    }
}