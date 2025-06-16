using HarmonyLib;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class PlayerAvatarPatches
{
    /// <summary>
    /// Detect when the player has died
    /// </summary>
    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeathDone))]
    [HarmonyPostfix]
    private static void OnPlayerDeath(PlayerAvatar __instance)
    {
        if (!__instance.isLocal || VRSession.Instance is not {} session)
            return;

        session.Player.Rig.SetVisible(false);
    }

    /// <summary>
    /// Detect when the player has been revived
    /// </summary>
    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.ReviveRPC))]
    [HarmonyPostfix]
    private static void OnPlayerRevive(PlayerAvatar __instance)
    {
        if (!__instance.isLocal || VRSession.Instance is not { } session)
            return;
        
        session.Player.Rig.SetVisible(true);

        // Reset CameraAimOffset (for when revived during the top-down death sequence)
        var offsetTransform = CameraAimOffset.Instance.transform;
        offsetTransform.localRotation = Quaternion.identity;
        offsetTransform.localPosition = Vector3.zero;
    }
}