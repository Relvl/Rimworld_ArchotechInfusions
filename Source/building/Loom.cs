using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Loom : AGridBuildingLinkable
{
    private LoomProps.Comp _comp;
    private LoomProps.Comp Comp => _comp ??= GetComp<LoomProps.Comp>();
}