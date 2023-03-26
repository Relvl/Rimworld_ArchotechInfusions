using ArchotechInfusions.building.proto;
using ArchotechInfusions.grid;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global, ClassNeverInstantiated.Global -- reflective
[StaticConstructorOnStartup]
public class ArchInf_Loom_Building : AddInf_Building
{
    private LoomGraphic _graphicCache;
    public LoomGraphic LoomGraphic => _graphicCache ??= new LoomGraphic("ArchotechInfusions/Things/Buildings/ArchInf_Loom_Atlas");

    public void Render(SectionLayer layer)
    {
        LoomGraphic.Render(layer, this);
    }

    public override void Print(SectionLayer layer)
    {
    }
}

// ReSharper disable once UnusedType.Global reflective: Verse.GraphicDatabase:GetInner[T]
public class LoomGraphic : Graphic_Linked
{
    private static readonly Vector2 Size = new(1f, 1f);

    public LoomGraphic()
    {
    }

    public LoomGraphic(string texPath) : this(GraphicDatabase.Get<Graphic_Single>(texPath, ShaderDatabase.Transparent))
    {
    }

    public LoomGraphic(Graphic subGraphic) : base(subGraphic)
    {
        this.subGraphic = subGraphic;
        data = subGraphic.data;
    }

    public override void Init(GraphicRequest req)
    {
        subGraphic = GraphicDatabase.Get<Graphic_Single>(req.graphicData.texPath, ShaderDatabase.Transparent);
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new LoomGraphic { subGraphic = subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), data = data };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!c.InBounds(parent.Map)) return false;
        if (!parent.Map.LightGrid().ShouldConnect(c, parent.TryGetComp<GridMemberComp>())) return false;
        return true;
    }

    public void Render(SectionLayer layer, Thing thing)
    {
        var position = thing.Position;
        var comp = thing.TryGetComp<GridMemberComp>();

        if (comp.Props.Visibility == GridVisibility.Never) return;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && position.GetTerrain(thing.Map).layerable) return;

        Print(layer, thing, 0);

        // todo Print extenders
        // for (var index = 0; index < 4; ++index)
        // {
        //     var intVec3 = position + GenAdj.CardinalDirections[index];
        //     if (ShouldDrawTo(intVec3, thing, comp))
        //         Printer_Plane.PrintPlane(layer, intVec3.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, LinkedDrawMatFrom(thing, intVec3));
        // }
    }

    private static bool ShouldDrawTo(IntVec3 c, Thing thing, GridMemberComp comp)
    {
        if (!c.InBounds(thing.Map)) return false;
        if (comp.Props.Visibility == GridVisibility.Never) return false;
        if (!c.InBounds(thing.Map)) return false;
        if (comp.Props.Visibility == GridVisibility.HideUnderTiling && c.GetTerrain(thing.Map).layerable) return false;
        if (!thing.Map.LightGrid().ShouldConnect(c, comp)) return false;
        return true;
    }
}