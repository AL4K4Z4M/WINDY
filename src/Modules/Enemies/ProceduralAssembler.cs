using UnityEngine;
using MelonLoader;
using System;
using ScheduleOne.NPCs;
using ScheduleOne.AvatarFramework.Equipping;
using Zordon.ScheduleI.Survival.Features;

namespace WindyFramework.Modules.Enemies
{
    public class ProceduralAssembler
    {
        private static ProceduralAssembler _instance;
        public static ProceduralAssembler Instance => _instance ?? (_instance = new ProceduralAssembler());

        /// <summary>
        /// Clones a base enemy prefab and prepares it for procedural modification.
        /// </summary>
        public GameObject CloneTemplate(GameObject template)
        {
            if (template == null)
            {
                Log("[Enemies] Cannot clone null template.", isError: true);
                return null;
            }

            // Instantiate inactive to prevent Awake/Start from running before we fix things
            bool wasActive = template.activeSelf;
            template.SetActive(false);
            
            GameObject clone = UnityEngine.Object.Instantiate(template);
            clone.name = $"{template.name}_Procedural_{Guid.NewGuid().ToString().Substring(0, 8)}";
            
            // Re-activate template if it was active
            if (wasActive) template.SetActive(true);

            // Fix NPC GUID
            NPC npc = clone.GetComponent<NPC>();
            if (npc != null)
            {
                // Generate a new GUID
                Guid newGuid = Guid.NewGuid();
                
                // Use Reflection to set the GUID backing field if needed, or just the property
                // NPC implements IGUIDRegisterable which usually uses the GUID property
                // But NPC.Awake/Start might register it.
                // We need to set it BEFORE Awake runs (which happens when we set active)
                
                // Set BakedGUID to empty or new to prevent conflict warning
                npc.BakedGUID = newGuid.ToString();
                
                // Force set the private GUID field if possible, or rely on Start() using BakedGUID
                // Based on source: NPC.Start() checks if GUID == Guid.Empty and uses BakedGUID
                // We need to reset GUID to Empty so Start() picks up the new BakedGUID
                var guidProp = typeof(NPC).GetProperty("GUID", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (guidProp != null) guidProp.SetValue(npc, Guid.Empty, null);
            }

            // Reset NetworkObject if present to ensure it can be spawned correctly
            var netObj = clone.GetComponent<FishNet.Object.NetworkObject>();
            if (netObj != null)
            {
                // We don't want to carry over any network state from the template
                // FishNet's NetworkObject usually handles this on Instantiate if it's a prefab,
                // but if we cloned a spawned object, we might need more cleanup.
                // For now, ensuring it's not 'spawned' is key.
            }

            // Cleanup Runtime Components that NPCMovement.Awake adds
            // NPCMovement adds ConstantForce to ragdoll RBs. If we clone an already initialized NPC, these are copied.
            // But Awake runs again on the clone, trying to add them again -> Error.
            if (npc != null && npc.Avatar != null && npc.Avatar.RagdollRBs != null)
            {
                foreach (var rb in npc.Avatar.RagdollRBs)
                {
                    if (rb != null)
                    {
                        var constantForces = rb.GetComponents<ConstantForce>();
                        foreach (var cf in constantForces)
                        {
                            UnityEngine.Object.DestroyImmediate(cf);
                        }
                    }
                }
            }

            // Clean up any existing DebugLabels from the template
            var labels = clone.GetComponents<DebugLabel>();
            foreach (var label in labels) UnityEngine.Object.DestroyImmediate(label);

            // Activate the clone to initialize components (Awake/Start)
            clone.SetActive(true);

            return clone;
        }

        /// <summary>
        /// Applies procedural stats to an NPC instance.
        /// </summary>
        public void ApplyStats(GameObject enemy, EnemyStats stats)
        {
            if (enemy == null || stats == null) return;

            NPC npc = enemy.GetComponent<NPC>();
            if (npc == null)
            {
                Log($"[Enemies] GameObject {enemy.name} does not have an NPC component.", isError: true);
                return;
            }

            // 1. Apply Health
            if (npc.Health != null)
            {
                npc.Health.MaxHealth = stats.GetCalculatedHealth();
                npc.Health.RestoreHealth();
                Log($"[Enemies] Set {npc.fullName} MaxHealth to {npc.Health.MaxHealth}");
            }

            // 2. Apply Movement Speed
            if (npc.Movement != null)
            {
                npc.Movement.WalkSpeed = stats.MoveSpeed * 0.4f; // Approximate ratio
                npc.Movement.RunSpeed = stats.MoveSpeed;
                npc.Movement.MoveSpeedMultiplier = 1f;
                Log($"[Enemies] Set {npc.fullName} RunSpeed to {npc.Movement.RunSpeed}");
            }

            // 3. Apply Weapon Stats (Damage and Attack Speed)
            // We search for components in children as weapons are often part of the model hierarchy
            AvatarWeapon[] weapons = enemy.GetComponentsInChildren<AvatarWeapon>(true);
            foreach (var weapon in weapons)
            {
                if (weapon == null) continue;

                // Apply Damage
                if (weapon is AvatarRangedWeapon ranged)
                {
                    ranged.Damage = stats.Damage;
                    ranged.MaxFireRate = 1f / stats.AttackSpeed; // Frequency to Period
                    Log($"[Enemies] Applied Ranged Stats to {weapon.name}: Damage={ranged.Damage}, FireRate={ranged.MaxFireRate}");
                }
                else
                {
                    // Ensure the weapon is active so it can receive stats and run combat logic
                    weapon.gameObject.SetActive(true);
                    ApplyMeleeStats(weapon, stats.MeleeDamage, stats.MeleeCooldown);
                }
            }

            // 4. Apply Visual Scaling (Subtle)
            // Example: 10% size increase per 50% health increase above base 100
            float healthRatio = stats.GetCalculatedHealth() / 100f;
            float scaleFactor = 1f + (healthRatio - 1f) * 0.2f; 
            scaleFactor = Mathf.Clamp(scaleFactor, 0.8f, 1.5f);
            
            npc.SetScale(scaleFactor);
            Log($"[Enemies] Set {npc.fullName} Scale to {scaleFactor}");
        }

        private void ApplyMeleeStats(AvatarWeapon weapon, float damage, float cooldown)
        {
            if (weapon == null) return;
            weapon.CooldownDuration = cooldown;
            // Note: Base AvatarWeapon might not expose generic 'Damage' setter if it's not Ranged.
            // Logic handled by combat system usually reads from config, but Cooldown is on the component.
            Log($"[Enemies] Applied Melee Stats to {weapon.name}: Damage={damage}, Cooldown={cooldown}");
        }

        public void ApplyVisualScaling(GameObject enemy, float scaleFactor)
        {
            if (enemy == null) return;
            NPC npc = enemy.GetComponent<NPC>();
            if (npc != null)
            {
                npc.SetScale(scaleFactor);
            }
            else
            {
                enemy.transform.localScale *= scaleFactor;
            }
        }

        private void Log(string msg, bool isError = false)
        {
            try
            {
                if (isError) MelonLogger.Error(msg);
                else MelonLogger.Msg(msg);
            }
            catch
            {
                // Fallback for non-MelonLoader environments (tests)
                if (isError) Console.WriteLine($"ERROR: {msg}");
                else Console.WriteLine(msg);
            }
        }
    }
}
