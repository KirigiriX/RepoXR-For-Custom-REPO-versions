using HarmonyLib;
using UnityEngine;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class WorldSpaceUIParentPatches
{
    /// <summary>
    /// Position elements actually in world space instead of screen space coordinates
    /// </summary>
    [HarmonyPatch(typeof(WorldSpaceUIChild), nameof(WorldSpaceUIChild.SetPosition))]
    [HarmonyPrefix]
    private static bool SetPositionPatch(WorldSpaceUIChild __instance)
    {
        var position = __instance.worldPosition + __instance.positionOffset;
        var direction = (position - AssetManager.instance.mainCamera.transform.position).normalized;
        var distance = Vector3.Distance(position, Camera.main!.transform.position);

        __instance.myRect.position = position;
        __instance.myRect.rotation = Quaternion.LookRotation(direction, Vector3.up);
        __instance.myRect.localScale = Vector3.one * Mathf.Lerp(1, 10, Mathf.InverseLerp(3, 20, distance));

        return false;
    }
}