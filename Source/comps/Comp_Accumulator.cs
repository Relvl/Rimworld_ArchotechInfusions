using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CompProps_Accumulator : CompPropertiesBase_Grid
{
    public float MaxStored;

    public CompProps_Accumulator()
    {
        compClass = typeof(Comp_Accumulator);
    }
}

public class Comp_Accumulator : CompBase_Grid<CompProps_Accumulator>;