using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyPatch(typeof(GenLabel))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Patch_GenLabel
{
    /// <summary>
    ///     Removes HP numbers for thing label if unbreakable.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GenLabel.LabelExtras))]
    public static bool LabelExtras_Prefix(Thing t, bool includeHp, bool includeQuality, ref string __result)
    {
        if (includeHp && t.TryGetInfusedComp(out var comp) && comp.IsUnbreakable)
        {
            __result = GenLabel.LabelExtras(t, false, includeQuality);
            return false;
        }

        return true;
    }
}