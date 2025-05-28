using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.defOf;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyPatch(typeof(ThingDef))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Patch_ThingDef
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
    public static void SpecialDisplayStats_Postfix(ThingDef __instance, StatRequest req, ref IEnumerable<StatDrawEntry> __result)
    {
        __result = new List<StatDrawEntry>(__result)
        {
            new(ArchInfStatCategoryDefOf.JAI_Pawn_Affected, ArchInfStatDefOf.JAI_Integrity)
        };
    }
}