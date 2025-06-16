using HarmonyLib;
using RepoXR.Player.Camera;
using UnityEngine;

namespace RepoXR.Patches.Item;

[RepoXRPatch]
internal static class MusicBoxTrapPatches
{
    /// <summary>
    /// Make the music box rotate the player in VR
    /// </summary>
    [HarmonyPatch(typeof(MusicBoxTrap), nameof(MusicBoxTrap.Update))]
    [HarmonyPostfix]
    private static void RotatePlayerPatch(MusicBoxTrap __instance)
    {
        if (!__instance.MusicBoxPlaying || !PhysGrabber.instance || PhysGrabber.instance.grabbedObject != __instance.rb)
            return;
        
        VRCameraAim.instance.TurnAimNow(90 * Time.deltaTime);
    }
}