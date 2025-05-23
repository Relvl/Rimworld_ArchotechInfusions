using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.grid;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.graphic;

public class GraphicGridOverlay(Graphic subGraphic) : Graphic_Linked(subGraphic)
{
    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (parent is not ThingWithComps thingWithComps) return false;
        if (!c.InBounds(parent.Map)) return false;
        var comp = thingWithComps.AllComps.FirstOrDefault(thingComp => thingComp is IBaseGridComp<CompPropertiesBase_Grid>) as IBaseGridComp<CompPropertiesBase_Grid>;
        if (comp is null) return false;
        // todo bull's shit! just check if this is one of mod buildings
        return parent.Map.ArchInfGrid().IsSameGrid(c, comp);
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        foreach (var cell in parent.OccupiedRect()) Printer_Plane.PrintPlane(layer, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay), Vector2.one, LinkedDrawMatFrom(parent, cell));
    }
}