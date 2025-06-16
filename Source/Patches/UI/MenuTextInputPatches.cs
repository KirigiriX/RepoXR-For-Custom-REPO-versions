using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.Assets;
using RepoXR.UI.Menu;
using UnityEngine;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.UI;

[RepoXRPatch]
internal static class MenuTextInputPatches
{
    /// <summary>
    /// Create a VR keyboard for any text input fields
    /// </summary>
    [HarmonyPatch(typeof(MenuTextInput), nameof(MenuTextInput.Start))]
    [HarmonyPostfix]
    private static void OnMenuTextInputCreate(MenuTextInput __instance)
    {
        var canvas = __instance.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        var keyboardContainer = new GameObject("Keyboard Scale")
            {
                transform =
                {
                    position = canvas.TransformPoint(new Vector3(canvas.sizeDelta.x / 2, -100, 0)),
                    rotation = canvas.rotation * Quaternion.Euler(15, 0, 0)
                }
            }
            .transform;
        var keyboard = Object.Instantiate(AssetCollection.Keyboard, keyboardContainer).transform;
        keyboard.localScale = Vector3.one * 0.0175f;

        keyboardContainer.gameObject.AddComponent<InputKeyboard>().menuInput = __instance;
    }

    /// <summary>
    /// Fix the cursor position in VR
    /// </summary>
    [HarmonyPatch(typeof(MenuTextInput), nameof(MenuTextInput.InputTextSet))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> TextCursorPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.position))))
            .SetOperandAndAdvance(PropertyGetter(typeof(Transform), nameof(Transform.localPosition)))
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, PropertySetter(typeof(Transform), nameof(Transform.position))))
            .SetOperandAndAdvance(PropertySetter(typeof(Transform), nameof(Transform.localPosition)))
            .InstructionEnumeration();
    }
    
    // Bunch of menu close detection patches

    [HarmonyPatch(typeof(MenuPageServerListCreateNew), nameof(MenuPageServerListCreateNew.ExitPage))]
    [HarmonyPostfix]
    private static void OnCreateServerButtonExit()
    {
        InputKeyboard.instance.Close();
    }

    [HarmonyPatch(typeof(MenuPageServerListCreateNew), nameof(MenuPageServerListCreateNew.ButtonConfirm))]
    [HarmonyPostfix]
    private static void OnCreateServerButtonConfirm()
    {
        InputKeyboard.instance.Close();
    }

    [HarmonyPatch(typeof(MenuPageServerListSearch), nameof(MenuPageServerListSearch.ExitPage))]
    [HarmonyPostfix]
    private static void OnSearchServerButtonExit()
    {
        InputKeyboard.instance.Close();
    }

    [HarmonyPatch(typeof(MenuPageServerListSearch), nameof(MenuPageServerListSearch.ButtonConfirm))]
    [HarmonyPostfix]
    private static void OnSearchServerButtonConfirm()
    {
        InputKeyboard.instance.Close();
    }
}