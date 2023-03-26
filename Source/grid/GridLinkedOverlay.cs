using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid;

public class GridLinkedOverlay : Graphic_Linked
{
    public GridLinkedOverlay(Graphic subGraphic) : base(subGraphic)
    {
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        return c.InBounds(parent.Map) && parent.Map.LightGrid().IsSameGrid(c, parent.TryGetComp<GridMemberComp>());
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        foreach (var cell in parent.OccupiedRect())
        {
            Printer_Plane.PrintPlane(layer, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay), Vector2.one, LinkedDrawMatFrom(parent, cell));
        }
    }
}