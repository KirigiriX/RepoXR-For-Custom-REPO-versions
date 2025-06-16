using HarmonyLib;
using RepoXR.Assets;
using TMPro;
using UnityEngine.UI;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class InventoryUIPatches
{
    /// <summary>
    /// Disable a bunch of the inventory UI since we use a hotbar-like system
    /// </summary>
    [HarmonyPatch(typeof(InventorySpot), nameof(InventorySpot.Start))]
    [HarmonyPostfix]
    private static void DisableBunchOfUIPatch(InventorySpot __instance)
    {
        __instance.GetComponent<RawImage>().enabled = false;
        __instance.GetComponentInChildren<Image>().enabled = false;
        __instance.GetComponentsInChildren<TextMeshProUGUI>().Do(comp => comp.enabled = false);
    }

    /// <summary>
    /// Disable the UI if it gets updated
    /// </summary>
    [HarmonyPatch(typeof(InventorySpot), nameof(InventorySpot.UpdateUI))]
    [HarmonyPostfix]
    private static void DisableUIOnceAgain(InventorySpot __instance)
    {
        __instance.inventoryIcon.enabled = false;
        __instance.noItem.enabled = false;
    }

    /// <summary>
    /// Make item info controls use the VR control scheme
    /// </summary>
    [HarmonyPatch(typeof(ItemInfoUI), nameof(ItemInfoUI.Start))]
    [HarmonyPostfix]
    private static void ItemInfoControlsPatch(ItemInfoUI __instance)
    {
        __instance.Text.spriteAsset = AssetCollection.TMPInputsSpriteAsset;
    }
}