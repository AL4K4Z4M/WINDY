using UnityEngine;
using MelonLoader;
using System.Collections.Generic;
using ScheduleOne.Effects;
using ScheduleOne.PlayerScripts;
using ScheduleOne.DevUtilities;

namespace Zordon.ScheduleI.Survival.Features
{
    public class DrugDebugModule : MonoBehaviour
    {
        private static DrugDebugModule _instance;
        public static DrugDebugModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DrugDebugModule");
                    Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<DrugDebugModule>();
                }
                return _instance;
            }
        }

        private Effect _activeEffect;
        private GUIStyle _effectStyle;
        private List<Effect> _allEffects = new List<Effect>();
        private float _announcementTimer = 0f;

        public List<Effect> GetAllEffects()
        {
            if (_allEffects.Count == 0)
            {
                string[] paths = { "Properties/Tier1", "Properties/Tier2", "Properties/Tier3", "Properties/Tier4", "Properties/Tier5" };
                foreach (var path in paths)
                {
                    var effects = Resources.LoadAll<Effect>(path);
                    if (effects != null) _allEffects.AddRange(effects);
                }

                // If still empty, try fallback root
                if (_allEffects.Count == 0)
                {
                    var fallback = Resources.LoadAll<Effect>("Effects");
                    if (fallback != null) _allEffects.AddRange(fallback);
                }

                // Sort by name for easier navigation
                _allEffects.Sort((a, b) => a.Name.CompareTo(b.Name));
                MelonLogger.Msg($"[DrugDebug] Loaded {_allEffects.Count} total effects.");
            }
            return _allEffects;
        }

        public void GiveRandomEffect()
        {
            var effects = GetAllEffects();
            if (effects.Count == 0) return;
            ApplyEffect(effects[Random.Range(0, effects.Count)]);
        }

        public void ApplyEffect(Effect effect)
        {
            if (Player.Local == null || effect == null) return;

            ClearActiveEffect();
            _activeEffect = effect;
            _announcementTimer = 4f; // Show "NEW EFFECT" announcement for 4s
            
            try
            {
                _activeEffect.ApplyToPlayer(Player.Local);
                MelonLogger.Msg($"[DrugDebug] Applied Effect: {_activeEffect.Name} ({_activeEffect.ID})");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[DrugDebug] Failed to apply effect {_activeEffect.Name}: {ex.Message}");
            }
        }

        public void ClearActiveEffect()
        {
            if (Player.Local == null) return;

            // 1. Clear our tracked debug effect
            if (_activeEffect != null)
            {
                try
                {
                    _activeEffect.ClearFromPlayer(Player.Local);
                    MelonLogger.Msg($"[DrugDebug] Cleared Effect: {_activeEffect.Name}");
                }
                catch {}
                _activeEffect = null;
            }

            // 2. Clear game's internal ConsumedProduct effects
            if (Player.Local.ConsumedProduct != null)
            {
                Player.Local.ConsumedProduct.ClearEffectsFromPlayer(Player.Local);
                var prop = typeof(Player).GetProperty("ConsumedProduct", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                prop?.SetValue(Player.Local, null, null);
            }
        }

        public void ClearEffectsFromNPC(ScheduleOne.NPCs.NPC npc)
        {
            if (npc == null || npc.Behaviour == null || npc.Behaviour.ConsumeProductBehaviour == null) return;

            // NPCs keep their effects in their ConsumeProductBehaviour
            var behav = npc.Behaviour.ConsumeProductBehaviour;
            if (behav.ConsumedProduct != null)
            {
                try
                {
                    behav.ConsumedProduct.ClearEffectsFromNPC(npc);
                    
                    // Clear the property using reflection since it has a private setter
                    var prop = typeof(ScheduleOne.NPCs.Behaviour.ConsumeProductBehaviour).GetProperty("ConsumedProduct", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    prop?.SetValue(behav, null, null);
                }
                catch {}
            }
        }

        private void Update()
        {
            if (_announcementTimer > 0) _announcementTimer -= Time.deltaTime;
        }

        private void OnGUI()
        {
            if (_activeEffect == null || _announcementTimer <= 0) return;

            if (_effectStyle == null)
            {
                _effectStyle = new GUIStyle();
                _effectStyle.alignment = TextAnchor.MiddleCenter;
                _effectStyle.fontSize = 64;
                _effectStyle.fontStyle = FontStyle.Bold;
                _effectStyle.richText = true;
            }

            // Calculate alpha for fade out (last 2 seconds of the 10s timer)
            float alpha = Mathf.Clamp01(_announcementTimer / 2f);
            
            string text = $"<color=yellow>NEW EFFECT APPLIED:</color>\n<color=white>{_activeEffect.Name.ToUpper()}</color>";
            
            // Draw Main with Alpha
            _effectStyle.normal.textColor = new Color(1, 1, 1, alpha);
            GUI.Label(new Rect(Screen.width / 2 - 400, Screen.height / 2 - 150, 800, 300), text, _effectStyle);
        }
    }
}