using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid.graphic;

public class GraphicGridOverlay : Graphic_Linked
{
    public GraphicGridOverlay(Graphic subGraphic) : base(subGraphic)
    {
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!c.InBounds(parent.Map)) return false;
        var comp = parent.TryGetComp<GridMemberComp>();
        if (comp is null) return false;
        return parent.Map.ArchInfGrid().IsSameGrid(c, parent.TryGetComp<GridMemberComp>());
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        foreach (var cell in parent.OccupiedRect())
        {
            Printer_Plane.PrintPlane(layer, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay), Vector2.one, LinkedDrawMatFrom(parent, cell));
        }
    }
}