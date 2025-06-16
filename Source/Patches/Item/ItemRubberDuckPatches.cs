using HarmonyLib;

namespace RepoXR.Patches.Item;

[RepoXRPatch]
internal static class ItemRubberDuckPatches
{
    /// <summary>
    /// Prevent the duck item from squeaking if it's equipped
    /// </summary>
    [HarmonyPatch(typeof(ItemRubberDuck), nameof(ItemRubberDuck.Squeak))]
    [HarmonyPrefix]
    private static bool NoSqueakIfEquipped(ItemRubberDuck __instance)
    {
        return __instance.itemEquippable.currentState != ItemEquippable.ItemState.Equipped;
    }
    
    /// <summary>
    /// Prevent the duck item from quacking if it's equipped
    /// </summary>
    [HarmonyPatch(typeof(ItemRubberDuck), nameof(ItemRubberDuck.Quack))]
    [HarmonyPrefix]
    private static bool NoQuackIfEquipped(ItemRubberDuck __instance)
    {
        return __instance.itemEquippable.currentState != ItemEquippable.ItemState.Equipped;
    }
    
    /// <summary>
    /// Prevent the duck item from quacking if it's equipped
    /// </summary>
    [HarmonyPatch(typeof(ItemRubberDuck), nameof(ItemRubberDuck.QuackRPC))]
    [HarmonyPrefix]
    private static bool NoQuackRPCIfEquipped(ItemRubberDuck __instance)
    {
        return __instance.itemEquippable.currentState != ItemEquippable.ItemState.Equipped;
    }
    
    /// <summary>
    /// Prevent the duck item from jumping if it's equipped
    /// </summary>
    [HarmonyPatch(typeof(ItemRubberDuck), nameof(ItemRubberDuck.LilQuackJump))]
    [HarmonyPrefix]
    private static bool NoJumpIfEquipped(ItemRubberDuck __instance)
    {
        return __instance.itemEquippable.currentState != ItemEquippable.ItemState.Equipped;
    }
}