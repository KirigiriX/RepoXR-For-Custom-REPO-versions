using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RepoXR.UI.Expressions;

using static HarmonyLib.AccessTools;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class ExpressionPatches
{
    /// <summary>
    /// Use the <see cref="ExpressionRadial.ExpressionActive" /> to see if an expression is active instead of using keybinds
    /// </summary>
    [HarmonyPatch(typeof(PlayerExpression), nameof(PlayerExpression.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RadialSelectionPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Call, Method(typeof(SemiFunc), nameof(SemiFunc.InputHold))))
            .Repeat(matcher =>
                matcher.SetOperandAndAdvance(
                    Method(typeof(ExpressionRadial), nameof(ExpressionRadial.ExpressionActive))))
            .InstructionEnumeration();
    }
}