using HarmonyLib;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.Patches.Enemy;

[RepoXRPatch]
internal static class EnemyBeamerPatches
{
    /// <summary>
    /// Apply haptic feedback when close to the beamer laser
    /// </summary>
    [HarmonyPatch(typeof(EnemyBeamer), nameof(EnemyBeamer.LaserLogic))]
    [HarmonyPostfix]
    private static void BeamerLaserHaptic(EnemyBeamer __instance)
    {
        if (__instance.currentState != EnemyBeamer.State.Attack)
            return;

        var playerPosition = PlayerAvatar.instance.transform.position;
        var distanceEnemy = Vector3.Distance(__instance.laserStartTransform.position, playerPosition);
        var distanceLaser = Vector3.Distance(__instance.hitPosition, playerPosition);
        var minDistance = Mathf.Min(distanceEnemy, distanceLaser);
        var factor = Mathf.InverseLerp(15, 3, minDistance);

        HapticManager.Impulse(HapticManager.Hand.Both, HapticManager.Type.Continuous, 0.6f * factor, priority: 2);
    }
}