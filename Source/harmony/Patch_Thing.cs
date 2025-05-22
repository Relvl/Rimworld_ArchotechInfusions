using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyPatch(typeof(Thing))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Patch_Thing
{
    /// <summary>
    ///     Allows Unbreakable instruction to prevent any apparel damage
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Thing.TakeDamage))]
    public static bool TakeDamage(Thing __instance)
    {
        if (__instance.TryGetInfusedComp(out var comp))
            if (comp.IsUnbreakable)
                return false;

        return true;
    }
}