using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using static HarmonyLib.AccessTools;

namespace RepoXR.Patches;

internal static class HarmonyPatcher
{
    private static readonly Harmony VRPatcher = new("io.daxcess.repoxr");
    private static readonly Harmony UniversalPatcher = new("io.daxcess.repoxr-universal");

    public static void PatchUniversal()
    {
        Patch(UniversalPatcher, RepoXRPatchTarget.Universal);
    }

    public static void PatchVR()
    {
        Patch(VRPatcher, RepoXRPatchTarget.VROnly);
    }

    public static void PatchClass(Type type)
    {
        UniversalPatcher.CreateClassProcessor(type, true).Patch();
    }

    private static void Patch(Harmony patcher, RepoXRPatchTarget target)
    {
        GetTypesFromAssembly(Assembly.GetExecutingAssembly()).Do(type =>
        {
            try
            {
                var attribute = (RepoXRPatchAttribute)Attribute.GetCustomAttribute(type, typeof(RepoXRPatchAttribute));

                if (attribute == null)
                    return;

                if (attribute.Dependency != null && !Compat.IsLoaded(attribute.Dependency))
                    return;

                if (attribute.Target != target)
                    return;
                
                Logger.LogDebug($"Applying patches from: {type.FullName}");

                patcher.CreateClassProcessor(type, true).Patch();
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to apply patches from {type}: {e.Message}, {e.InnerException}");
            }
        });
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal class RepoXRPatchAttribute(RepoXRPatchTarget target = RepoXRPatchTarget.VROnly, string? dependency = null)
    : Attribute
{
    public RepoXRPatchTarget Target { get; } = target;
    public string? Dependency { get; } = dependency;
}

internal enum RepoXRPatchTarget
{
    Universal,
    VROnly
}

/// <summary>
/// Fixes a bug in older BepInEx versions (shame on you TS for using a 2-year-old BepInEx)
///
/// https://github.com/BepInEx/HarmonyX/commit/2ea021afcf1811c9f4a4a05b02ba4c7fa188ad9d#diff-43c8b1e327cd0788a5aaa5d683148d079b959c7fba68afcc5d3ccf43dbd6c4bfL322
/// </summary>
[RepoXRPatch(RepoXRPatchTarget.Universal)]
[HarmonyPriority(Priority.First)]
internal static class LeaveMyLeaveAlonePatch
{
    [UsedImplicitly]
    private static MethodBase TargetMethod()
    {
        var type = TypeByName("HarmonyLib.Internal.Patching.ILManipulator");
        var method = Method(type, "WriteTo");

        return method;
    }

    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldsfld, Field(typeof(OpCodes), nameof(OpCodes.Leave))))
            .Advance(-2)
            .RemoveInstructions(22)
            .InstructionEnumeration();
    }
}