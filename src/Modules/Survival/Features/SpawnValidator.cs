using UnityEngine;
using UnityEngine.AI;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using System.Collections.Generic;

namespace Zordon.ScheduleI.Survival.Features
{
    public static class SpawnValidator
    {
        /// <summary>
        /// Validates if a position is suitable for spawning an enemy.
        /// Checks: NavMesh, Reachability to at least one player, and physical obstructions.
        /// </summary>
        public static bool IsValidSpawnPoint(Vector3 position, out Vector3 finalPosition)
        {
            finalPosition = position;

            // 1. NavMesh Validation
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(position, out hit, 5.0f, NavMesh.AllAreas))
            {
                MelonLoader.MelonLogger.Warning($"[SpawnValidator] Position {position} failed NavMesh sampling.");
                return false;
            }
            finalPosition = hit.position;

            // 2. Reachability Check (to any active player)
            bool reachable = false;
            foreach (var player in Player.PlayerList)
            {
                if (player == null || player.Health == null || !player.Health.IsAlive) continue;

                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(finalPosition, player.transform.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        reachable = true;
                        break;
                    }
                    else if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        // Calculate path length
                        float pathLen = 0f;
                        if (path.corners.Length > 1)
                        {
                            for (int i = 1; i < path.corners.Length; i++)
                                pathLen += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                        }

                        float directDist = Vector3.Distance(finalPosition, player.transform.position);
                        
                        // Accept partial path if it covers at least 50% of the distance or is reasonably long (>10m)
                        if (pathLen > directDist * 0.5f || pathLen > 10f)
                        {
                            reachable = true;
                            break;
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Warning($"[SpawnValidator] Rejected Partial Path. Len: {pathLen:F1}, Direct: {directDist:F1}");
                        }
                    }
                }
            }

            if (!reachable) 
            {
                MelonLoader.MelonLogger.Warning($"[SpawnValidator] Position {finalPosition} is not reachable by any player.");
                return false;
            }

            // 3. Obstruction Check (Sphere check for clearance)
            // We use a small radius to ensure they don't spawn inside walls/props
            if (Physics.CheckSphere(finalPosition + Vector3.up * 1f, 0.5f, LayerMask.GetMask("Default", "Terrain")))
            {
                MelonLoader.MelonLogger.Warning($"[SpawnValidator] Position {finalPosition} is obstructed.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to find a valid spawn point from a list of candidates.
        /// </summary>
        public static bool TryFindValidSpawnPoint(List<Vector3> candidates, out Vector3 validPoint)
        {
            foreach (var pos in candidates)
            {
                if (IsValidSpawnPoint(pos, out validPoint))
                {
                    return true;
                }
            }

            validPoint = Vector3.zero;
            return false;
        }
    }
}
