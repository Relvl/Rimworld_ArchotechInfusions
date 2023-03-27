using System.Linq;
using ArchotechInfusions.building.proto;
using Verse;

namespace ArchotechInfusions.grid.graphic;

// ReSharper disable once UnusedType.Global -- reflective: Verse.Section:ctor -- generates always with ctor(Section)
public class SectionLayerGridRender : SectionLayer
{
    public SectionLayerGridRender(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlag.Buildings;
    }

    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);

        foreach (var c in section.CellRect)
        foreach (var buildingPipe in Map.thingGrid.ThingsListAt(c).OfType<ArchInf_BuildingLink>())
            buildingPipe.PrintLinkable(this);

        FinalizeMesh(MeshParts.All);
    }
}