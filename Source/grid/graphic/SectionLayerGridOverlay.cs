using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using Verse;

namespace ArchotechInfusions.graphic;

[SuppressMessage("ReSharper", "UnusedType.Global")] // instantiates by Rimworld
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
            if (!thingDef.comps.Any(c => typeof(IGridComp<CompProperties>).IsAssignableFrom(c.compClass))) return;
            base.DrawLayer();
            return;
        }

        if (GridMapComponent.GridToDebug != null) base.DrawLayer();
    }

    /// <summary>
    ///     Calls only when terrain data changed
    /// </summary>
    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        foreach (var c in section.CellRect)
            Map.ArchInfGrid().RenderOverlay(this, c);
        FinalizeMesh(MeshParts.All);
    }
}