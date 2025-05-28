using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class AccumulatorProps : CompProperties
{
    public float MaxStored;

    public AccumulatorProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<AccumulatorProps>;
}