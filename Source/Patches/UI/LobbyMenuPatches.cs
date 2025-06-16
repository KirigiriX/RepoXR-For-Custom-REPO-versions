using HarmonyLib;
using RepoXR.Assets;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class LobbyMenuPatches
{
    /// <summary>
    /// Force the lobby page chat text to use our custom binding sprites
    /// </summary>
    [HarmonyPatch(typeof(MenuPageLobby), nameof(MenuPageLobby.UpdateChatPrompt))]
    [HarmonyPostfix]
    private static void UpdateChatPromptSprites(MenuPageLobby __instance)
    {
        __instance.chatPromptText.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
    }
}