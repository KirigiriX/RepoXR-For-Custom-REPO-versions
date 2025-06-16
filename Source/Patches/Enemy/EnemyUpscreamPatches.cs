using HarmonyLib;
using RepoXR.Player.Camera;

namespace RepoXR.Patches.Enemy;

[RepoXRPatch]
internal static class EnemyUpscreamPatches
{
    /// <summary>
    /// Apply VR camera aim and zoom when being attacked by the Upscream
    /// </summary>
    [HarmonyPatch(typeof(EnemyUpscream), nameof(EnemyUpscream.Update))]
    [HarmonyPostfix]
    private static void OnUpscreamAttack(EnemyUpscream __instance)
    {
        if (__instance.currentState != EnemyUpscream.State.Attack || !__instance.targetPlayer ||
            !__instance.targetPlayer.isLocal)
            return;

        VRCameraAim.instance.SetAimTarget(__instance.visionTransform.position, 0.1f, 5, __instance.gameObject, 90,
            Plugin.Config.ReducedAimImpact.Value);
        VRCameraZoom.instance.SetZoomTarget(-0.15f, 0.1f, 5f, 5f, __instance.visionTransform, 50);
    }
}