using ArchotechInfusions.building.proto;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.graphic;

public class GraphicGridLink(Graphic subGraphic) : GraphicLinkedMoreLayers(subGraphic)
{
    private static readonly Vector2 Size = new(1f, 1f);

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new GraphicGridLink(subGraphic) { data = data };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (parent is not AGridBuilding gridBuilding) return false;
        return c.InBounds(parent.Map) && parent.Map.ArchInfGrid().ShouldConnect(c, gridBuilding);
    }

    public void PrintLinkable(SectionLayer layer, AGridBuildingLinkable loom)
    {
        if (ArchotechInfusionsMod.Settings.HideLoomBelowFlooring && loom.Position.GetTerrain(loom.Map).layerable) return;

        Printer_Plane.PrintPlane(layer, loom.TrueCenter(), Size, LinkedDrawMatFrom(loom, loom.Position));

        // Print extenders
        for (var index = 0; index < 4; ++index)
        {
            var sideCell = loom.Position + GenAdj.CardinalDirections[index];
            if (ShouldDrawTo(sideCell, loom))
                Printer_Plane.PrintPlane(layer, sideCell.ToVector3ShiftedWithAltitude(loom.def.Altitude), Vector2.one, LinkedDrawMatFrom(loom, sideCell));
        }
    }

    private static bool ShouldDrawTo(IntVec3 c, AGridBuildingLinkable loom)
    {
        if (!c.InBounds(loom.Map)) return false;
        if (ArchotechInfusionsMod.Settings.HideLoomBelowFlooring && c.GetTerrain(loom.Map).layerable) return false;
        if (!loom.Map.ArchInfGrid().ShouldConnect(c, loom)) return false;
        return true;
    }
}