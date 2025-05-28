using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class LoomProps : CompProperties
{
    public LoomProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<LoomProps>;
}