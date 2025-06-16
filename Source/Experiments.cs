using HarmonyLib;

namespace RepoXR;

#if DEBUG
internal static class Experiments
{
    [HarmonyPatch(typeof(EnemyDirector), nameof(EnemyDirector.Awake))]
    [HarmonyPostfix]
    private static void FuckLolEnemy(EnemyDirector __instance)
    {
        // Only allow eyeyeyeyeyeye spawning
        var enemy = __instance.enemiesDifficulty1[0];

        // Only allow mouth spawning
        // var enemy = __instance.enemiesDifficulty1[4];

        // Only allow thin-man spawning
        // var enemy = __instance.enemiesDifficulty1[1];

        // Only allow upSCREAM! spawning
        // var enemy = __instance.enemiesDifficulty2[2];

        // Only allow beamer spawning
        // var enemy = __instance.enemiesDifficulty3[4];

        __instance.enemiesDifficulty1.Clear();
        __instance.enemiesDifficulty2.Clear();
        __instance.enemiesDifficulty3.Clear();

        __instance.enemiesDifficulty1.Add(enemy);
        __instance.enemiesDifficulty2.Add(enemy);
        __instance.enemiesDifficulty3.Add(enemy);
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
    [HarmonyPostfix]
    private static void InfiniteSprintPatch(PlayerController __instance)
    {
        __instance.EnergyCurrent = __instance.EnergyStart;
    }

    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
    // [HarmonyPrefix]
    private static bool NoDamage()
    {
        return false;
    }

    [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.DebugDev))]
    [HarmonyPrefix]
    private static bool SemiWorkHehehehahahahahahEhehehehahaha(ref bool __result)
    {
        __result = true;
        return false;
    }

    private static bool done;

    [HarmonyPatch(typeof(MenuButton), nameof(MenuButton.OnHovering))]
    [HarmonyPrefix]
    private static void HeheMuseum()
    {
        if (done)
            return;

        done = true;

        var mgr = RunManager.instance;

        mgr.levels.RemoveRange(1, 3);
        mgr.levels[0].NarrativeName = "Wie dit leest is gek";

        Logger.LogDebug(mgr.levels[0].ValuablePresets[0].small.Count);

        var logoTiny = mgr.levels[0].ValuablePresets[0].tiny[^2];
        var headsetTiny = mgr.levels[0].ValuablePresets[0].tiny[^1];

        var logoSmall = mgr.levels[0].ValuablePresets[0].small[^2];
        var headsetSmall = mgr.levels[0].ValuablePresets[0].small[^1];

        var logoMedium = mgr.levels[0].ValuablePresets[0].medium[^2];
        var headsetMedium = mgr.levels[0].ValuablePresets[0].medium[^1];

        mgr.levels[0].ValuablePresets[0].tiny.Clear();
        mgr.levels[0].ValuablePresets[0].small.Clear();
        mgr.levels[0].ValuablePresets[0].medium.Clear();

        mgr.levels[0].ValuablePresets[0].tiny.AddRange([logoTiny, headsetTiny]);
        mgr.levels[0].ValuablePresets[0].small.AddRange([logoSmall, headsetSmall]);
        mgr.levels[0].ValuablePresets[0].medium.AddRange([logoMedium, headsetMedium]);
    }
}
#endif