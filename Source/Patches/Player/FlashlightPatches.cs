using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Networking;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class FlashlightPatches
{
    /// <summary>
    /// Disable a few of the flashlight scripts locally since it doesn't really work well with VR
    /// </summary>
    [HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Start))]
    [HarmonyPrefix]
    private static void OnFlashlightStart(FlashlightController __instance)
    {
        if (!__instance.PlayerAvatar.IsVRPlayer())
            return;

        __instance.toolBackAway.enabled = false;
        __instance.GetComponentInChildren<FlashlightBob>().enabled = false;
        __instance.GetComponentInChildren<FlashlightSprint>().enabled = false;
        __instance.GetComponentInChildren<FlashlightTilt>().enabled = false;
    }
}

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class UniversalFlashlightPatches
{
    /// <summary>
    /// Prevent the flashlight from being repositioned when held by a VR player
    /// </summary>
    [HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DisableFlashlightMovement(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.isLocal))))
            .SetInstruction(new CodeInstruction(OpCodes.Call, ((Func<PlayerAvatar, bool>)IsVRPlayerOrLocal).Method))
            .InstructionEnumeration();
        
        static bool IsVRPlayerOrLocal(PlayerAvatar player)
        {
            return player.isLocal || player.IsVRPlayer();
        }
    }

    /// <summary>
    /// Prevent script from changing flashlight rotations on VR players
    /// </summary>
    [HarmonyPatch(typeof(FlashlightLightAim), nameof(FlashlightLightAim.Update))]
    [HarmonyPostfix]
    private static void OnFlashlightAim(FlashlightLightAim __instance)
    {
        if (!__instance.playerAvatar.IsVRPlayer())
            return;

        __instance.enabled = false;
        __instance.transform.localRotation = Quaternion.identity;
        __instance.transform.parent.localRotation = Quaternion.identity;
    }
}