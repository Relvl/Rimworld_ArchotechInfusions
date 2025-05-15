using ArchotechInfusions.building.proto;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid.graphic;

public class GraphicGridLink : GraphicLinkedMoreLayers
{
    private static readonly Vector2 Size = new(1f, 1f);

    public GraphicGridLink(Graphic subGraphic) : base(subGraphic)
    {
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new GraphicGridLink(subGraphic) { data = data };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!c.InBounds(parent.Map)) return false;
        var parentComp = parent.TryGetComp<GridMemberComp>();
        if (!parent.Map.ArchInfGrid().ShouldConnect(c, parentComp)) return false;
        return true;
    }

    public void PrintLinkable(SectionLayer layer, ArchInf_BuildingLink thing)
    {
        var position = thing.Position;
        var comp = thing.TryGetComp<GridMemberComp>();
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

    private static bool ShouldDrawTo(IntVec3 c, Thing thing, GridMemberComp comp)
    {
        if (!c.InBounds(thing.Map)) return false;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && c.GetTerrain(thing.Map).layerable) return false;
        if (!thing.Map.ArchInfGrid().ShouldConnect(c, comp)) return false;
        return true;
    }
}