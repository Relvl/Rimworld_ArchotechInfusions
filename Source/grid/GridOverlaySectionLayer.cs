using System.Linq;
using RimWorld;
using Verse;

namespace ArchotechInfusions.grid;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global -- reflective: Verse.Section:ctor -- generates always with ctor(Section)
public class GridOverlaySectionLayer : SectionLayer
{
    public GridOverlaySectionLayer(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlag.Buildings;
    }

    public override void DrawLayer()
    {
        if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator)
        {
            if (designator.PlacingDef is not ThingDef thingDef) return;
            if (!thingDef.comps.OfType<GridMemberCompProps>().Any()) return;
            base.DrawLayer();
        }
    }

    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        foreach (var c in section.CellRect) Map.LightGrid().RenderOverlay(this, c);
        FinalizeMesh(MeshParts.All);
    }
}