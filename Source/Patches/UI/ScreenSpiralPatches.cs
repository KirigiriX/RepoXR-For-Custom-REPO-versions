using HarmonyLib;
using Unity.XR.CoreUtils;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class ScreenSpiralPatches
{
    /// <summary>
    /// Small improvements for the hypnosis UI in VR
    /// </summary>
    [HarmonyPatch(typeof(SpiralOnScreen), nameof(SpiralOnScreen.Start))]
    [HarmonyPostfix]
    private static void OnSpiralCreate(SpiralOnScreen __instance)
    {
        __instance.gameObject.SetLayerRecursively(5); // Render on top of EVERYTHING
    }
}