using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Input;
using RepoXR.Managers;
using RepoXR.Networking;
using RepoXR.Player;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class PhysGrabberPatches
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Transform GetHandTransform()
    {
        if (VRSession.Instance is { } session)
            return session.Player.MainHand;

        return PhysGrabber.instance.playerCamera.transform;
    }

    private static CodeMatcher ReplaceCameraWithHand(this CodeMatcher matcher)
    {
        var labels = matcher.Instruction.labels;

        return matcher.RemoveInstructions(2).InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, ((Func<Transform>)GetHandTransform).Method).WithLabels(labels)
        );
    }

    /// <summary>
    /// Make certain phys grabber operations operate from the hand transform instead of the camera transform
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> UpdatePatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Repeat(matcher => matcher.Advance(-1).ReplaceCameraWithHand())
            .Start()
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerVisionTarget))))
            .Advance(-2)
            .RemoveInstructions(4)
            .Insert(
                new CodeInstruction(OpCodes.Call, ((Func<Transform>)GetHandTransform).Method)
            )
            .InstructionEnumeration();
    }

    /// <summary>
    /// Slow down the push/pull logic since it's way too fast in VR
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PushPullSlowdownPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.2f))
            .SetOperandAndAdvance(0.1f)
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(InputManager), nameof(InputManager.instance))),
                new CodeInstruction(OpCodes.Callvirt,
                    Method(typeof(InputManager), nameof(InputManager.KeyPullAndPush))),
                new CodeInstruction(OpCodes.Call,
                    Method(typeof(Vector3), "op_Multiply", [typeof(Vector3), typeof(float)]))
            )
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.2f))
            .SetOperandAndAdvance(-0.1f)
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(InputManager), nameof(InputManager.instance))),
                new CodeInstruction(OpCodes.Callvirt,
                    Method(typeof(InputManager), nameof(InputManager.KeyPullAndPush))),
                new CodeInstruction(OpCodes.Call,
                    Method(typeof(Vector3), "op_Multiply", [typeof(Vector3), typeof(float)]))
            )
            .InstructionEnumeration();
    }

    /// <summary>
    /// Make sure the <see cref="PhysGrabber.physGrabPointPlane"/> and <see cref="PhysGrabber.physGrabPointPuller"/> are
    /// manually updated if we are holding something.
    ///
    /// This is normally done by having these be a child of the camera, however this doesn't work in VR since
    /// we use our hand to move items, not the main camera.
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyPostfix]
    private static void UpdatePhysGrabPlane(PhysGrabber __instance)
    {
        if (!__instance.isLocal || !__instance.grabbedObjectTransform)
            return;

        var hand = GetHandTransform();
        var distancePlane = Vector3.Distance(hand.position, __instance.physGrabPointPlane.position);
        var distancePuller = Vector3.Distance(hand.position, __instance.physGrabPointPuller.position);

        __instance.physGrabPointPlane.position = hand.position + hand.forward * distancePlane;
        __instance.physGrabPointPuller.position = hand.position + hand.forward * distancePuller;
    }

    /// <summary>
    /// Provide haptic feedback while something is grabbed
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyPostfix]
    private static void HapticFeedbackPatch(PhysGrabber __instance)
    {
        if (!__instance.isLocal)
            return;

        var grabbed = __instance.grabbed
            ? AssetCollection.GrabberHapticCurve.EvaluateTimed(__instance.loopSound.Source.pitch * 1.12667f) * 0.1f
            : 0;
        var overcharge = __instance.physGrabBeamOverChargeFloat * 0.4f *
                         AssetCollection.OverchargeHapticCurve.EvaluateTimed(__instance.physGrabBeamOverChargeFloat *
                                                                             3);

        if (grabbed + overcharge <= 0)
            return;

        HapticManager.Impulse(HapticManager.Hand.Dominant, HapticManager.Type.Continuous, grabbed + overcharge);
    }

    /// <summary>
    /// When grabbing items, shoot rays out of the hand, instead of the camera
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.RayCheck))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RayCheckPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    Method(typeof(Physics), nameof(Physics.Raycast),
                    [
                        typeof(Vector3), typeof(Vector3), typeof(RaycastHit).MakeByRefType(), typeof(float),
                        typeof(int),
                        typeof(QueryTriggerInteraction)
                    ])))
            .Advance(-10)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, ((Func<PhysGrabber, Vector3>)CalculateNewForward).Method),
                new CodeInstruction(OpCodes.Stloc_1),
                new CodeInstruction(OpCodes.Ldarg_0)
            )
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Repeat(matcher => matcher.Advance(-1).ReplaceCameraWithHand())
            .Start()
            .MatchForward(false, new CodeMatch(OpCodes.Call, PropertyGetter(typeof(Camera), nameof(Camera.main))))
            .Repeat(matcher => matcher.ReplaceCameraWithHand())
            .InstructionEnumeration();

        static Vector3 CalculateNewForward(PhysGrabber grabber)
        {
            if (grabber.overrideGrab && grabber.overrideGrabTarget)
                return (grabber.overrideGrabTarget.transform.position - VRSession.Instance.Player.MainHand.position)
                    .normalized;

            return VRSession.Instance is not { } session ? Vector3.zero : session.Player.MainHand.forward;
        }
    }

    /// <summary>
    /// Make "scrolling" update the position based on the hand, instead of the camera
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.OverridePullDistanceIncrement))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OverridePullDistanceIncrementPatches(
        IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .Advance(-1)
            .ReplaceCameraWithHand()
            .InstructionEnumeration();
    }

    /// <summary>
    /// Make the object turning input use the controller inputs instead of mouse inputs
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.ObjectTurning))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ObjectTurningPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            // Use VR controls for rotation instead of mouse inputs
            .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Mouse X"))
            .RemoveInstructions(26)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0), // this
                new CodeInstruction(OpCodes.Ldloc_0), // max(mass, 1)
                new CodeInstruction(OpCodes.Call, ((Func<PhysGrabber, float, Vector3>)GetRotationInput).Method)
            )
            .InstructionEnumeration();

        static Vector3 GetRotationInput(PhysGrabber grabber, float mass)
        {
            var input = Actions.Instance["Rotation"].ReadValue<Vector2>();
            var massMult = Mathf.Lerp(0.1f, 1, mass * 0.05f);

            return new Vector3(input.x * massMult, input.y * massMult, 0) * 15 * grabber.grabStrength;
        }
    }

    /// <summary>
    /// Move the grab beam origin to the hand
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabBeam), nameof(PhysGrabBeam.Start))]
    [HarmonyPostfix]
    private static void OnPhysBeamStart(PhysGrabBeam __instance)
    {
        if (!__instance.playerAvatar.isLocal || VRSession.Instance is not {} session)
            return;

        __instance.PhysGrabPointOrigin.SetParent(session.Player.MainHand);
        __instance.PhysGrabPointOrigin.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Allow a custom override to disable object turning
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.ObjectTurning))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DisableTurningPatch(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Call, Method(typeof(SemiFunc), nameof(SemiFunc.InputHold))))
            .Advance(1);

        var jmp = matcher.Instruction;

        matcher.Advance(1).InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
            new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRSession), nameof(VRSession.Player))),
            new CodeInstruction(OpCodes.Ldfld, Field(typeof(VRPlayer), nameof(VRPlayer.disableRotateTimer))),
            new CodeInstruction(OpCodes.Ldc_R4, 0f),
            new CodeInstruction(OpCodes.Bgt_Un_S, jmp.operand)
        );

        return matcher
            .InstructionEnumeration();
    }

    /// <summary>
    /// Detect item release and try to equip item if possible
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.ReleaseObject))]
    [HarmonyPrefix]
    private static void OnReleaseObject(PhysGrabber __instance)
    {
        if (!__instance.grabbed || !__instance.isLocal || !__instance.grabbedObject ||
            !__instance.grabbedObject.TryGetComponent<ItemEquippable>(out var item))
            return;

        if (VRSession.Instance is not { } session)
            return;

        session.Player.Rig.inventoryController.TryEquipItem(item);
    }

    private static float forceGrabTimer;

    /// <summary>
    /// Every time a grab override is triggered, reset the timer
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.OverrideGrab))]
    [HarmonyPostfix]
    private static void OnOverrideGrab(PhysGrabber __instance)
    {
        forceGrabTimer = 0.1f;
    }

    /// <summary>
    /// If the <see cref="forceGrabTimer"/> is above zero, do not allow the grabber to let go
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ForceOverrideGrabPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Stfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.overrideGrabTarget))))
            .Advance(-13)
            .SetInstruction(new CodeInstruction(OpCodes.Call, ((Func<PhysGrabber, bool>)CheckAndUpdate).Method))
            .InstructionEnumeration();

        static bool CheckAndUpdate(PhysGrabber grabber)
        {
            forceGrabTimer -= Time.deltaTime;

            return grabber.overrideGrab && forceGrabTimer <= 0;
        }
    }
}

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class PhysGrabberUniversalPatches
{
    private static Transform GetHandTransform(PhysGrabber grabber)
    {
        if (grabber.playerAvatar.isLocal)
            return VRSession.InVR ? VRSession.Instance.Player.MainHand : grabber.playerAvatar.localCameraTransform;

        if (!NetworkSystem.instance)
        {
            Logger.LogError("NetworkSystem is null?");
            return grabber.playerAvatar.localCameraTransform;
        }

        if (NetworkSystem.instance.GetNetworkPlayer(grabber.playerAvatar, out var networkPlayer))
        {
            if (!networkPlayer)
            {
                Logger.LogError("NetworkPlayer is null?");
                return grabber.playerAvatar.localCameraTransform;
            }

            if (!networkPlayer.PrimaryHand)
            {
                Logger.LogError("GrabberHand is null?");
                return grabber.playerAvatar.localCameraTransform;
            }

            return networkPlayer.PrimaryHand;
        }

        return grabber.playerAvatar.localCameraTransform;
    }

    /// <summary>
    /// Make certain phys grabber logic be applied based on the hand instead of the head
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.PhysGrabLogic))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PhysGrabLogicPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PhysGrabber), nameof(PhysGrabber.playerCamera))))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                ((Func<PhysGrabber, Transform>)GetHandTransform).Method))
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(PhysGrabber), nameof(PhysGrabber.ObjectTurning))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ObjectTurningPatches(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            // Replace camera transform with hand transform (local player)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.localCameraTransform))))
            .Advance(-1)
            .RemoveInstructions(2)
            .Insert(new CodeInstruction(OpCodes.Call, ((Func<PhysGrabber, Transform>)GetHandTransform).Method))
            // Replace camera transform with hand transform (remote player)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, Field(typeof(PlayerAvatar), nameof(PlayerAvatar.localCameraTransform))))
            .Advance(-1)
            .RemoveInstructions(2)
            .Insert(new CodeInstruction(OpCodes.Call,
                ((Func<PhysGrabber, Transform>)GetHandTransform).Method))
            .InstructionEnumeration();
    }
}