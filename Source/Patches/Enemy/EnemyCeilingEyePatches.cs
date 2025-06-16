﻿using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Managers;
using RepoXR.Player.Camera;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.Enemy;

[RepoXRPatch]
internal static class EnemyCeilingEyePatches
{
    /// <summary>
    /// Replace <see cref="CameraAim.AimTargetSoftSet"/> with <see cref="VRCameraAim.SetAimTargetSoft"/>
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetCameraSoftRotationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, Method(typeof(CameraAim), nameof(CameraAim.AimTargetSoftSet))))
            .Advance(-11)
            .SetOperandAndAdvance(Field(typeof(VRCameraAim), nameof(VRCameraAim.instance)))
            .Advance(10)
            // Make the rotation less severe if reduced aim impact is enabled
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Plugin.GetConfigGetter()))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt,
                PropertyGetter(typeof(Config), nameof(Config.ReducedAimImpact))))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt,
                PropertyGetter(typeof(ConfigEntry<bool>), nameof(ConfigEntry<bool>.Value))))
            .SetOperandAndAdvance(Method(typeof(VRCameraAim), nameof(VRCameraAim.SetAimTargetSoft)))
            .InstructionEnumeration();
    }

    /// <summary>
    /// Replace <see cref="CameraAim.AimTargetSet"/> with <see cref="VRCameraAim.SetAimTarget"/>
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.UpdateStateRPC))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetCameraRotationPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, Method(typeof(CameraAim), nameof(CameraAim.AimTargetSet))))
            .Advance(-10)
            .SetOperandAndAdvance(Field(typeof(VRCameraAim), nameof(VRCameraAim.instance)))
            .Advance(9)
            // Make the rotation less severe if reduced aim impact is enabled
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Plugin.GetConfigGetter()))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt,
                PropertyGetter(typeof(Config), nameof(Config.ReducedAimImpact))))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt,
                PropertyGetter(typeof(ConfigEntry<bool>), nameof(ConfigEntry<bool>.Value))))
            .SetOperandAndAdvance(Method(typeof(VRCameraAim), nameof(VRCameraAim.SetAimTarget)))
            .InstructionEnumeration();
    }

    /// <summary>
    /// Provide haptic feedback while attached to the ceiling eye
    /// </summary>
    [HarmonyPatch(typeof(EnemyCeilingEye), nameof(EnemyCeilingEye.Update))]
    [HarmonyPostfix]
    private static void EyeAttachHapticFeedback(EnemyCeilingEye __instance)
    {
        if (__instance.currentState != EnemyCeilingEye.State.HasTarget)
            return;
        
        if (!__instance.targetPlayer || !__instance.targetPlayer.isLocal)
            return;

        // Want a higher priority than damage, so we use 11
        HapticManager.Impulse(HapticManager.Hand.Both, HapticManager.Type.Continuous,
            0.2f * AssetCollection.EyeAttachHapticCurve.EvaluateTimed(0.25f), priority: 11);
    }
}