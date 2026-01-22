using UnityEngine;
using ScheduleOne.NPCs;
using MelonLoader;

namespace Zordon.ScheduleI.Survival.Features
{
    public class EnemyMonitor : MonoBehaviour
    {
        private NPC _npc;
        private Vector3 _lastPosition;
        private float _lastCheckTime;
        private const float CHECK_INTERVAL = 5f;

        public void Setup(NPC npc)
        {
            _npc = npc;
            _lastPosition = transform.position;
            _lastCheckTime = Time.time;
        }

        private void Update()
        {
            if (_npc == null || _npc.Health.IsDead) return;

            if (Time.time - _lastCheckTime > CHECK_INTERVAL)
            {
                // Safety Check: Don't flag as stuck if close to any player (likely in combat/engagement)
                if (SurvivalController.Instance != null)
                {
                    var nearest = SurvivalController.Instance.GetNearestPlayer(transform.position);
                    if (nearest != null && Vector3.Distance(transform.position, nearest.transform.position) < 30f)
                    {
                        // Too close to player, assume combat holding behavior
                        _lastPosition = transform.position;
                        _lastCheckTime = Time.time;
                        return;
                    }
                }

                if (EnemyMonitorLogic.IsStuck(transform.position, _lastPosition, Time.time - _lastCheckTime))
                {
                    // Handle Stuck
                    HandleStuck();
                }

                _lastPosition = transform.position;
                _lastCheckTime = Time.time;
            }
        }

        private void HandleStuck()
        {
            // For Phase 1, we just log. In Phase 3, we'll implement teleport.
            MelonLogger.Msg($"[Survival] Enemy {_npc.fullName} appears stuck. Moved < 0.5m in {CHECK_INTERVAL}s.");
            
            // Temporary fix: If really stuck, maybe nudge or damage? 
            // Phase 3 will add teleport logic.
        }
    }
}
