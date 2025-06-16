using HarmonyLib;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class ArrowPatches
{
    /// <summary>
    /// Disable arrow UI (it isn't used in the base game, but is annoyingly visible in VR)
    /// </summary>
    [HarmonyPatch(typeof(ArrowUI), nameof(ArrowUI.Awake))]
    [HarmonyPostfix]
    private static void OnArrowUICreate(ArrowUI __instance)
    {
        __instance.enabled = false;
    }
}