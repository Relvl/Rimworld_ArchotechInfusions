using ArchotechInfusions.building.proto;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.graphic;

public class GraphicGridOverlay(Graphic subGraphic) : Graphic_Linked(subGraphic)
{
    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (parent is not AGridBuilding gridBuilding) return false;
        return c.InBounds(parent.Map) && parent.Map.ArchInfGrid().IsSameGrid(c, gridBuilding);
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        foreach (var cell in parent.OccupiedRect())
            Printer_Plane.PrintPlane(layer, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay), Vector2.one, LinkedDrawMatFrom(parent, cell));
    }
}