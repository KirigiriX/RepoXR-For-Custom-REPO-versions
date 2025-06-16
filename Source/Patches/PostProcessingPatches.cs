using HarmonyLib;

namespace RepoXR.Patches;

[RepoXRPatch]
internal static class PostProcessingPatches
{
    /// <summary>
    /// Disable certain post-processing effects that do not work well with VR
    /// </summary>
    [HarmonyPatch(typeof(PostProcessing), nameof(PostProcessing.Start))]
    [HarmonyPostfix]
    private static void PostProcessingPatch(PostProcessing __instance)
    {
        __instance.lensDistortion.active = false;
        __instance.chromaticAberration.active = false;
    }

    [HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.UpdateLensDistortion))]
    [HarmonyPrefix]
    private static bool DisableLensDistortion()
    {
        return false;
    }

    [HarmonyPatch(typeof(GraphicsManager), nameof(GraphicsManager.UpdateChromaticAberration))]
    [HarmonyPrefix]
    private static bool DisableChromaticAberration()
    {
        return false;
    }
}