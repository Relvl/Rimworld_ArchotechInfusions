using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.graphic;

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
            if (!thingDef.comps.Any(cp => cp is CompPropertiesBase_Grid)) return;
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
        foreach (var c in section.CellRect) Map.ArchInfGrid().RenderOverlay(this, c);
        FinalizeMesh(MeshParts.All);
    }
}