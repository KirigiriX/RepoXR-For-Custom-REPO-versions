using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Managers;
using RepoXR.UI.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace RepoXR.Patches.UI;

[RepoXRPatch(RepoXRPatchTarget.Universal)]
internal static class MenuPatches
{
    /// <summary>
    /// Register custom menus when the menu managers boots up
    /// </summary>
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Awake))]
    [HarmonyPrefix]
    private static void OnMenuManagerStart(MenuManager __instance)
    {
        if (MenuManager.instance)
            return;
        
        MenuHelper.RegisterCustomMenus(__instance);
    }
    
    /// <summary>
    /// Insert VR settings into the settings menu
    /// </summary>
    [HarmonyPatch(typeof(MenuPageSettings), nameof(MenuPageSettings.Start))]
    [HarmonyPostfix]
    private static void AddVRSettingsPatch(MenuPageSettings __instance)
    {
        // On PC, only add the VR settings menu on the main menu
        if (!SemiFunc.MenuLevel() && !VRSession.InVR)
            return;
        
        var vrButton = Object.Instantiate(AssetCollection.VRSettingsButton, __instance.transform)
            .GetComponent<Button>();
        var vrRect = vrButton.GetComponent<RectTransform>();

        // Brute forcing element positions
        vrRect.anchoredPosition = new Vector2(126, 83);
        vrButton.onClick.AddListener(() =>
        {
            __instance.ButtonEventBack();
            MenuHelper.PageOpenOnTop(MenuHelper.RepoXRMenuPage.VRSettings);
        });

        // Move back button a bit down
        __instance.transform.Find("Menu Button - Back").GetComponent<RectTransform>().anchoredPosition =
            new Vector2(126, 35);
    }
}