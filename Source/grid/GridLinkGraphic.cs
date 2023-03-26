using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid;

// ReSharper disable once UnusedType.Global reflective: Verse.GraphicDatabase:GetInner[T]
public class GridLinkGraphic : Graphic_Linked
{
    private static readonly Vector2 Size = new(1f, 1f);

    public override void Init(GraphicRequest req)
    {
        subGraphic = GraphicDatabase.Get<Graphic_Single>(req.graphicData.texPath, ShaderDatabase.Transparent);
        data = subGraphic.data;
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new GridLinkGraphic { subGraphic = subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), data = data };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!c.InBounds(parent.Map)) return false;
        if (!parent.Map.LightGrid().ShouldConnect(c, parent.TryGetComp<GridMemberComp>())) return false;
        return true;
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        var position = thing.Position;
        var comp = thing.TryGetComp<GridMemberComp>();
        if (comp.Props.Visibility == GridVisibility.Never) return;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && position.GetTerrain(thing.Map).layerable) return;

        Printer_Plane.PrintPlane(layer, thing.TrueCenter(), Size, LinkedDrawMatFrom(thing, thing.Position), extraRotation);

        // todo Print extenders
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
        if (!thing.Map.LightGrid().ShouldConnect(c, comp)) return false;
        return true;
    }
}