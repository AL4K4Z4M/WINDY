using UnityEngine;

namespace Zordon.ScheduleI.Survival.Features
{
    public static class EnemyMonitorLogic
    {
        public static bool IsStuck(Vector3 currentPos, Vector3 lastPos, float timeDelta)
        {
            if (timeDelta < 1f) return false;
            float dist = Vector3.Distance(currentPos, lastPos);
            
            // If moved less than 0.5m in the timeDelta (which is likely > 5s), it's stuck
            // Exception: If we wanted them to stop? No, survival enemies should always be moving towards player.
            
            return dist < 0.5f;
        }
    }
}
