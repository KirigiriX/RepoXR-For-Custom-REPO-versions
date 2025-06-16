using HarmonyLib;
using RepoXR.Rendering;

namespace RepoXR.Patches.Rendering;

[RepoXRPatch]
internal static class PostProcessPatches
{
    /// <summary>
    /// Add a custom post-processing manager on the base game's post-processing volume
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(PostProcessing), nameof(PostProcessing.Awake))]
    [HarmonyPostfix]
    private static void OnPostProcessCreated(PostProcessing __instance)
    {
        __instance.gameObject.AddComponent<CustomPostProcessing>();
    }
}