using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CompProperties_Loom : CompPropertiesBase_Grid
{
    public CompProperties_Loom()
    {
        compClass = typeof(Comp_Loom);
    }
}

public class Comp_Loom : CompBase_Grid<CompProperties_Loom>;