using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class PlayerHealthPatches
{
    /// <summary>
    /// Make the VR rig inherit the hurt animation from the base game
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Update))]
    [HarmonyPostfix]
    private static void OnPlayerHealthUpdate(PlayerHealth __instance)
    {
        if (!__instance.playerAvatar.isLocal || VRSession.Instance is not { } session)
            return;

        if (!__instance.materialEffect)
            session.Player.Rig.SetHurtAmount(0);
        else
            session.Player.Rig.SetHurtAmount(__instance.materialEffectCurve.Evaluate(__instance.materialEffectLerp));
    }

    /// <summary>
    /// Make sure the rig has a red damage color if the player gets hurt
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
    [HarmonyPostfix]
    private static void OnPlayerHurt(PlayerHealth __instance, int damage)
    {
        if (__instance.invincibleTimer > 0 || damage <= 0 ||
            (GameManager.Multiplayer() && !__instance.photonView.IsMine))
            return;

        if (__instance.playerAvatar.deadSet || __instance.godMode || __instance.health <= 0)
        {
            OnSetMaterialGreen(__instance);
            return;
        }

        if (VRSession.Instance is not { } session)
            return;

        __instance.materialEffect = true;
        __instance.materialEffectLerp = 0;

        session.Player.Rig.SetHurtColor(Color.red);
        
        AssetCollection.HurtHapticCurve.Pulse(HapticManager.Hand.Both, 0.8f, 2, 10);
    }

    /// <summary>
    /// Basically a postfix patch, but I need access to one of the local variables, see static inner function for more info
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.MaterialEffectOverrideRPC))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MaterialEffectOverridePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .End()
            .Advance(-1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, ((Action<PlayerHealth, Color>)OnMaterialEffectOverride).Method)
            )
            .InstructionEnumeration();

        // Pass the color given to MaterialEffectOverride to the arms rig (if this is our player)
        static void OnMaterialEffectOverride(PlayerHealth health, Color newColor)
        {
            if (!health.playerAvatar.isLocal || VRSession.Instance is not { } session)
                return;

            session.Player.Rig.SetHurtColor(newColor);
        }
    }

    /// <summary>
    /// Pass the green color through to the local arms rig
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.SetMaterialGreen))]
    [HarmonyPostfix]
    private static void OnSetMaterialGreen(PlayerHealth __instance)
    {
        if (!__instance.playerAvatar.isLocal || VRSession.Instance is not { } session)
            return;

        session.Player.Rig.SetHurtColor(new Color(0, 1, 0.25f));
    }
}