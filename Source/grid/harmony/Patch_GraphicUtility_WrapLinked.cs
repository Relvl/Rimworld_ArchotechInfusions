using ArchotechInfusions.grid.graphic;
using HarmonyLib;
using Verse;

namespace ArchotechInfusions.grid.harmony;

// ReSharper disable once InconsistentNaming, UnusedType.Global
[HarmonyPatch(typeof(GraphicUtility), nameof(GraphicUtility.WrapLinked))]
public class Patch_GraphicUtility_WrapLinked
{
    // ReSharper disable once InconsistentNaming, UnusedMember.Local
    private static bool Prefix(Graphic subGraphic, ref Graphic_Linked __result)
    {
        if (subGraphic is GraphicLinkedMoreLayers moreLayers)
        {
            __result = moreLayers;
            return false;
        }

        return true;
    }
}