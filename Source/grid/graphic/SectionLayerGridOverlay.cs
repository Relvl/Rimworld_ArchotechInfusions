using System.Linq;
using RimWorld;
using Verse;

namespace ArchotechInfusions.grid.graphic;

// ReSharper disable once UnusedType.Global -- reflective: Verse.Section:ctor -- generates always with ctor(Section)
public class SectionLayerGridOverlay : SectionLayer
{
    public SectionLayerGridOverlay(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlagDefOf.Buildings;
    }

    public override void DrawLayer()
    {
        if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator)
        {
            if (designator.PlacingDef is not ThingDef thingDef) return;
            if (!thingDef.comps.OfType<GridMemberCompProps>().Any()) return;
            base.DrawLayer();
            return;
        }

        if (GridMapComponent.DebudGrid != null)
        {
            base.DrawLayer();
            return;
        }
    }

    /// <summary>
    /// Calls only when terrain data changed
    /// </summary>
    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        foreach (var c in section.CellRect) Map.LightGrid().RenderOverlay(this, c);
        FinalizeMesh(MeshParts.All);
    }
}