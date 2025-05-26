using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyDebug]
[HarmonyPatch(typeof(StatsReportUtility))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Patch_StatReportUtility
{
    /// <summary>
    ///     Draws "Unbreakable" instead of thing hit points value also as description in the infocard.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("StatsToDraw", typeof(Thing))]
    public static void StatsToDraw(Thing thing, ref IEnumerable<StatDrawEntry> __result)
    {
        if (thing.TryGetInfusedComp(out var comp) && comp.IsUnbreakable)
        {
            var result = new List<StatDrawEntry>();
            foreach (var entry in __result)
                if (entry.DisplayPriorityWithinCategory == 99998 && entry.category == StatCategoryDefOf.BasicsImportant)
                    result.Add(
                        new StatDrawEntry(
                            entry.category,
                            "HitPointsBasic".Translate().CapitalizeFirst(),
                            $"<color=green>{"JAI.instruction.unbreakable".Translate()}</color>",
                            "JAI.instruction.unbreakable.infocard".Translate(),
                            99998
                        )
                    );
                else
                    result.Add(entry);
            __result = result;
        }
    }
}