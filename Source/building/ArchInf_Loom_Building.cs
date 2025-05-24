using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using ArchotechInfusions.grid.graphic;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ArchInf_Loom_Building : AGridBuildingLinkable
{
    private Comp_Loom _comp;
    private Comp_Loom Comp => _comp ??= GetComp<Comp_Loom>();
    public override GridVisibility Visibility => Comp.Props.Visibility;
}