using HarmonyLib;
using RepoXR.Player.Camera;

namespace RepoXR.Patches.Enemy;

[RepoXRPatch]
internal static class EnemyThinManPatches
{
    /// <summary>
    /// Apply VR camera aim and zoom when being attacked by Thin Man
    /// </summary>
    [HarmonyPatch(typeof(EnemyThinManAnim), nameof(EnemyThinManAnim.NoticeSet))]
    [HarmonyPostfix]
    private static void OnThinManAttack(EnemyThinManAnim __instance)
    {
        if (__instance.enemy.Health.healthCurrent < 0 || !__instance.controller.playerTarget.isLocal)
            return;

        VRCameraZoom.instance.SetZoomTarget(2, 0.75f, 3, 1, __instance.controller.head.transform, 90);
        VRCameraAim.instance.SetAimTarget(__instance.controller.head.transform.position, 0.75f, 2,
            __instance.controller.gameObject, 90, true);
    }
}