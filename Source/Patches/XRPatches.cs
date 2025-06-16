using HarmonyLib;
using UnityEngine.InputSystem.XR;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class XRPatches
{
    /// <summary>
    /// Fixes some issues with the input system
    /// </summary>
    [HarmonyPatch(typeof(XRSupport), nameof(XRSupport.Initialize))]
    [HarmonyPrefix]
    private static bool OnBeforeInitialize()
    {
        return false;
    }
}