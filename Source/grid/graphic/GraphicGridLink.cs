using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.grid;
using ArchotechInfusions.grid.graphic;
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
        if (parent is not ThingWithComps thingWithComps) return false;
        if (!c.InBounds(parent.Map)) return false;
        var comp = thingWithComps.AllComps.FirstOrDefault(thingComp => thingComp is IBaseGridComp<CompPropertiesBase_Grid>) as IBaseGridComp<CompPropertiesBase_Grid>;
        return parent.Map.ArchInfGrid().ShouldConnect(c, comp);
    }

    public void PrintLinkable(SectionLayer layer, ArchInf_BuildingLink thing)
    {
        var comp = thing.AllComps.FirstOrDefault(thingComp => thingComp is IBaseGridComp<CompPropertiesBase_Grid>) as IBaseGridComp<CompPropertiesBase_Grid>;
        if (comp is null) return;
        var position = thing.Position;
        if (comp.Props.Visibility == GridVisibility.Never) return;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && position.GetTerrain(thing.Map).layerable) return;

        Printer_Plane.PrintPlane(layer, thing.TrueCenter(), Size, LinkedDrawMatFrom(thing, thing.Position));

        // Print extenders
        for (var index = 0; index < 4; ++index)
        {
            var sideCell = position + GenAdj.CardinalDirections[index];
            if (ShouldDrawTo(sideCell, thing, comp))
                Printer_Plane.PrintPlane(layer, sideCell.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, LinkedDrawMatFrom(thing, sideCell));
        }
    }

    private static bool ShouldDrawTo(IntVec3 c, Thing thing, IBaseGridComp<CompPropertiesBase_Grid> comp)
    {
        if (!c.InBounds(thing.Map)) return false;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && c.GetTerrain(thing.Map).layerable) return false;
        if (!thing.Map.ArchInfGrid().ShouldConnect(c, comp)) return false;
        return true;
    }
}