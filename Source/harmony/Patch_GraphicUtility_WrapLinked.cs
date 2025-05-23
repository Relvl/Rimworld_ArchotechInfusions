using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.graphic;
using HarmonyLib;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyPatch(typeof(GraphicUtility), nameof(GraphicUtility.WrapLinked))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class Patch_GraphicUtility_WrapLinked
{
    /// <summary>
    /// </summary>
    private static bool Prefix(Graphic subGraphic, ref Graphic_Linked __result)
    {
        if (subGraphic is not GraphicLinkedMoreLayers moreLayers) return true;
        __result = moreLayers;
        return false;
    }
}