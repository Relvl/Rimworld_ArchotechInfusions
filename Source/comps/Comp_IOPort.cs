using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CompProperties_IOPort : CompPropertiesBase_Grid
{
    public CompProperties_IOPort()
    {
        compClass = typeof(Comp_IOPort);
    }
}

public class Comp_IOPort : CompBase_Grid<CompProperties_IOPort>
{
}