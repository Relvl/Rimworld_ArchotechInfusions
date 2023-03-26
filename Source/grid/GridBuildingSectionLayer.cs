using System.Linq;
using ArchotechInfusions.building;
using Verse;

namespace ArchotechInfusions.grid;

// ReSharper disable once UnusedType.Global -- reflective
public class GridBuildingSectionLayer : SectionLayer
{
    public GridBuildingSectionLayer(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlag.Buildings;
    }

    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        foreach (var c in section.CellRect)
        foreach (var building in Map.thingGrid.ThingsListAt(c).OfType<ArchInf_Loom_Building>())
            building.Render(this);
        FinalizeMesh(MeshParts.All);
    }
}